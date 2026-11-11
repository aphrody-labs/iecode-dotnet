using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Markdig;
using IEVRModManager.Helpers;
using IEVRModManager.Managers;

namespace IEVRModManager.Windows
{
    /// <summary>
    /// Interaction logic for AppUpdateWindow.xaml. Displays update information and provides link to GameBanana.
    /// </summary>
    public partial class AppUpdateWindow : Window
    {
        private readonly AppUpdateManager.ReleaseInfo _releaseInfo;
        private readonly string _currentVersion;
        private const string GameBananaUrl = "https://gamebanana.com/tools/21354";

        /// <summary>
        /// Initializes a new instance of the <see cref="AppUpdateWindow"/> class.
        /// </summary>
        /// <param name="owner">The owner window.</param>
        /// <param name="releaseInfo">The release information to display.</param>
        /// <param name="currentVersion">The current application version.</param>
        public AppUpdateWindow(Window owner, AppUpdateManager.ReleaseInfo releaseInfo, string currentVersion)
        {
            InitializeComponent();
            Owner = owner;
            _releaseInfo = releaseInfo;
            _currentVersion = currentVersion;
            
            UpdateLocalizedTexts();
            Loaded += (s, e) => UpdateLocalizedTexts();
        }

        private void UpdateLocalizedTexts()
        {
            Title = LocalizationHelper.GetString("AppUpdateAvailable");
            TitleText.Text = LocalizationHelper.GetString("AppUpdateAvailable");
            
            var message = string.Format(
                LocalizationHelper.GetString("AppUpdateAvailableMessage"),
                _currentVersion,
                _releaseInfo.TagName.TrimStart('v', 'V')
            );
            MessageText.Text = message;

            if (!string.IsNullOrWhiteSpace(_releaseInfo.Body))
            {
                ReleaseNotesLabel.Text = LocalizationHelper.GetString("ReleaseNotes") + ":";
                ReleaseNotesLabel.Visibility = Visibility.Visible;
                
                // Convert markdown to FlowDocument
                var flowDocument = ConvertMarkdownToFlowDocument(_releaseInfo.Body);
                ReleaseNotesViewer.Document = flowDocument;
                ReleaseNotesViewer.Visibility = Visibility.Visible;
            }

            OpenGameBananaButton.Content = LocalizationHelper.GetString("OpenGameBananaPage");
            LaterButton.Content = LocalizationHelper.GetString("Later");
        }

        private void OpenGameBananaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = GameBananaUrl,
                    UseShellExecute = true
                });
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(LocalizationHelper.GetString("CouldNotOpenLink"), ex.Message),
                    LocalizationHelper.GetString("ErrorTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void LaterButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Converts markdown text to a FlowDocument for display in WPF.
        /// </summary>
        private FlowDocument ConvertMarkdownToFlowDocument(string markdown)
        {
            var flowDocument = new FlowDocument
            {
                Background = Brushes.Transparent,
                FontSize = 11
            };

            try
            {
                // Get theme text color
                var textColor = Application.Current.Resources["TextColor"];
                var textBrush = textColor is Color color ? new SolidColorBrush(color) : Brushes.White;
                flowDocument.Foreground = textBrush;

                // Use Markdig to convert markdown to HTML
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
                var html = Markdown.ToHtml(markdown, pipeline);
                
                // Convert HTML to FlowDocument using simple parser
                var converter = new HtmlToFlowDocumentConverter(textBrush);
                flowDocument = converter.Convert(html);
            }
            catch
            {
                // Fallback: show markdown as plain text if conversion fails
                var textColor = Application.Current.Resources["TextColor"];
                var textBrush = textColor is Color color ? new SolidColorBrush(color) : Brushes.White;
                
                var paragraph = new Paragraph(new Run(markdown))
                {
                    Margin = new Thickness(0),
                    Foreground = textBrush
                };
                flowDocument.Blocks.Add(paragraph);
                flowDocument.Foreground = textBrush;
            }

            return flowDocument;
        }

        /// <summary>
        /// Simple HTML to FlowDocument converter for basic markdown rendering.
        /// </summary>
        private class HtmlToFlowDocumentConverter
        {
            private readonly Brush _textBrush;

            public HtmlToFlowDocumentConverter(Brush textBrush)
            {
                _textBrush = textBrush;
            }

            public FlowDocument Convert(string html)
            {
                var document = new FlowDocument
                {
                    Background = Brushes.Transparent,
                    Foreground = _textBrush,
                    FontSize = 11
                };

                // Simple HTML parsing - handle common markdown HTML elements
                var lines = html.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                Paragraph? currentParagraph = null;

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    
                    if (string.IsNullOrWhiteSpace(trimmed))
                    {
                        if (currentParagraph != null)
                        {
                            document.Blocks.Add(currentParagraph);
                            currentParagraph = null;
                        }
                        continue;
                    }

                    // Handle headings
                    if (trimmed.StartsWith("<h1>", StringComparison.OrdinalIgnoreCase))
                    {
                        if (currentParagraph != null)
                        {
                            document.Blocks.Add(currentParagraph);
                        }
                        var text = StripHtmlTags(trimmed);
                        currentParagraph = new Paragraph(new Run(text))
                        {
                            FontSize = 16,
                            FontWeight = FontWeights.Bold,
                            Margin = new Thickness(0, 8, 0, 4),
                            Foreground = _textBrush
                        };
                        document.Blocks.Add(currentParagraph);
                        currentParagraph = null;
                    }
                    else if (trimmed.StartsWith("<h2>", StringComparison.OrdinalIgnoreCase))
                    {
                        if (currentParagraph != null)
                        {
                            document.Blocks.Add(currentParagraph);
                        }
                        var text = StripHtmlTags(trimmed);
                        currentParagraph = new Paragraph(new Run(text))
                        {
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Margin = new Thickness(0, 6, 0, 4),
                            Foreground = _textBrush
                        };
                        document.Blocks.Add(currentParagraph);
                        currentParagraph = null;
                    }
                    else if (trimmed.StartsWith("<h3>", StringComparison.OrdinalIgnoreCase))
                    {
                        if (currentParagraph != null)
                        {
                            document.Blocks.Add(currentParagraph);
                        }
                        var text = StripHtmlTags(trimmed);
                        currentParagraph = new Paragraph(new Run(text))
                        {
                            FontSize = 12,
                            FontWeight = FontWeights.Bold,
                            Margin = new Thickness(0, 4, 0, 2),
                            Foreground = _textBrush
                        };
                        document.Blocks.Add(currentParagraph);
                        currentParagraph = null;
                    }
                    // Handle lists
                    else if (trimmed.StartsWith("<li>", StringComparison.OrdinalIgnoreCase))
                    {
                        if (currentParagraph == null)
                        {
                            currentParagraph = new Paragraph { Foreground = _textBrush };
                        }
                        var text = StripHtmlTags(trimmed);
                        currentParagraph.Inlines.Add(new Run("• " + text + Environment.NewLine) { Foreground = _textBrush });
                    }
                    // Handle paragraphs and other content
                    else if (trimmed.StartsWith("<p>", StringComparison.OrdinalIgnoreCase) || 
                             trimmed.StartsWith("<strong>", StringComparison.OrdinalIgnoreCase) ||
                             trimmed.StartsWith("<em>", StringComparison.OrdinalIgnoreCase) ||
                             !trimmed.StartsWith("<"))
                    {
                        if (currentParagraph == null)
                        {
                            currentParagraph = new Paragraph { Foreground = _textBrush, Margin = new Thickness(0, 2, 0, 2) };
                        }
                        
                        // Parse inline formatting
                        ParseInlineFormatting(trimmed, currentParagraph);
                    }
                }

                if (currentParagraph != null)
                {
                    document.Blocks.Add(currentParagraph);
                }

                return document;
            }

            private void ParseInlineFormatting(string html, Paragraph paragraph)
            {
                // Simple approach: extract text from HTML tags and add as plain text
                // Markdig already converted markdown to HTML, so we just need to extract text
                var text = StripHtmlTags(html);
                
                if (!string.IsNullOrWhiteSpace(text))
                {
                    paragraph.Inlines.Add(new Run(text) { Foreground = _textBrush });
                }
                
                paragraph.Inlines.Add(new Run(Environment.NewLine));
            }

            private string StripHtmlTags(string html)
            {
                var text = html;
                // Remove HTML tags but preserve content
                text = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", "");
                // Decode HTML entities
                text = text.Replace("&nbsp;", " ")
                          .Replace("&lt;", "<")
                          .Replace("&gt;", ">")
                          .Replace("&amp;", "&")
                          .Replace("&quot;", "\"")
                          .Replace("&#39;", "'")
                          .Replace("&apos;", "'");
                return text.Trim();
            }
        }
    }
}
