using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using IEVRModManager;
using IEVRModManager.Helpers;

namespace IEVRModManager.Windows
{
    /// <summary>
    /// Interaction logic for DownloadsWindow.xaml. Displays download links for mods and tools.
    /// </summary>
    public partial class DownloadsWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadsWindow"/> class.
        /// </summary>
        /// <param name="parent">The parent window.</param>
        public DownloadsWindow(Window parent)
        {
            InitializeComponent();
            Owner = parent;
            
            // Update localized texts
            UpdateLocalizedTexts();
        }
        
        private void UpdateLocalizedTexts()
        {
            Title = LocalizationHelper.GetString("Downloads");
            TitleLabel.Content = LocalizationHelper.GetString("DownloadLinks");
            GameBananaLink.Text = LocalizationHelper.GetString("DownloadModsFromGameBanana");
            ViolaLink.Text = LocalizationHelper.GetString("DownloadViolaCLI");
            CpkListLink.Text = LocalizationHelper.GetString("CpkListRepository");
            DownloadCpkListButton.Content = LocalizationHelper.GetString("DownloadCpkList");
            CloseButton.Content = LocalizationHelper.GetString("Close");
        }

        private void Link_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBlock textBlock)
            {
                textBlock.TextDecorations = System.Windows.TextDecorations.Underline;
            }
        }

        private void Link_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBlock textBlock)
            {
                textBlock.TextDecorations = null;
            }
        }

        private void ViolaLink_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Config.ViolaReleaseUrl,
                UseShellExecute = true
            });
        }

        private void CpkListLink_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Config.CpkListUrl,
                UseShellExecute = true
            });
        }

        private void DownloadCpkListButton_Click(object sender, RoutedEventArgs e)
        {
            if (Owner is MainWindow mainWindow)
            {
                mainWindow.DownloadCpkLists_Click(sender, e);
            }
            else
            {
                // Fallback: open the link if owner is not available
                CpkListLink_Click(sender, null!);
            }
        }

        private void GameBananaLink_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Config.GameBananaModsUrl,
                UseShellExecute = true
            });
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

