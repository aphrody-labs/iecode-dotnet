using System.Windows;
using System.Windows.Threading;
using IEVRModManager.Helpers;

namespace IEVRModManager.Windows
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml. Displays progress for long-running operations.
    /// </summary>
    public partial class ProgressWindow : Window
    {
        private readonly Dispatcher _dispatcher;
        private bool _allowClose = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressWindow"/> class.
        /// </summary>
        /// <param name="parent">The parent window.</param>
        public ProgressWindow(Window parent)
        {
            InitializeComponent();
            Owner = parent;
            _dispatcher = Dispatcher;
            
            // Update localized texts
            Title = LocalizationHelper.GetString("ApplyingMods");
            TitleText.Text = LocalizationHelper.GetString("ApplyingMods");
            
            // Prevent closing while processing, unless explicitly allowed
            Closing += (s, e) =>
            {
                if (!_allowClose)
                {
                    e.Cancel = true;
                }
            };
        }

        /// <summary>
        /// Allows the window to be closed.
        /// </summary>
        public void AllowClose()
        {
            _allowClose = true;
            Close();
        }

        /// <summary>
        /// Updates the progress percentage and status message.
        /// </summary>
        /// <param name="percentage">The progress percentage (0-100).</param>
        /// <param name="status">The status message to display.</param>
        public void UpdateProgress(int percentage, string status)
        {
            _dispatcher.Invoke(() =>
            {
                ProgressBar.Value = percentage;
                StatusText.Text = status;
                PercentageText.Text = $"{percentage}%";
            });
        }

        /// <summary>
        /// Sets whether the progress bar should be indeterminate (animated) or show specific progress.
        /// </summary>
        /// <param name="indeterminate"><c>true</c> to show indeterminate progress; otherwise, <c>false</c>.</param>
        public void SetIndeterminate(bool indeterminate)
        {
            _dispatcher.Invoke(() =>
            {
                if (indeterminate)
                {
                    ProgressBar.IsIndeterminate = true;
                    PercentageText.Text = "";
                }
                else
                {
                    ProgressBar.IsIndeterminate = false;
                }
            });
        }
    }
}

