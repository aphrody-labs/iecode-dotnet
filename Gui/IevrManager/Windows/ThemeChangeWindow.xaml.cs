using System.Windows;
using System.Windows.Input;
using IEVRModManager.Helpers;

namespace IEVRModManager.Windows
{
    /// <summary>
    /// Interaction logic for ThemeChangeWindow.xaml. Prompts user to restart after theme change.
    /// </summary>
    public partial class ThemeChangeWindow : Window
    {
        /// <summary>
        /// Gets whether the user chose to restart the application.
        /// </summary>
        public bool UserChoseRestart { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeChangeWindow"/> class.
        /// </summary>
        /// <param name="owner">The owner window.</param>
        /// <param name="themeName">The name of the theme that was changed to.</param>
        public ThemeChangeWindow(Window owner, string themeName)
        {
            InitializeComponent();
            Owner = owner;
            
            // Update localized texts
            Title = LocalizationHelper.GetString("ThemeChanged");
            TitleText.Text = LocalizationHelper.GetString("ThemeChanged");
            MessageText.Text = string.Format(LocalizationHelper.GetString("ThemeChangedMessage"), themeName);
            RestartButton.Content = LocalizationHelper.GetString("RestartNow");
            LaterButton.Content = LocalizationHelper.GetString("Later");
            
            // Close with Escape key
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    Later_Click(null!, null!);
                }
            };

            Loaded += (s, e) =>
            {
                RestartButton.Focus();
            };
        }

        private void RestartNow_Click(object sender, RoutedEventArgs e)
        {
            UserChoseRestart = true;
            DialogResult = true;
            Close();
        }

        private void Later_Click(object sender, RoutedEventArgs e)
        {
            UserChoseRestart = false;
            DialogResult = false;
            Close();
        }
    }
}

