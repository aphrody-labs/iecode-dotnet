using System.Windows;
using IEVRModManager.Helpers;

namespace IEVRModManager.Windows
{
    /// <summary>
    /// Interaction logic for UpdateDetectedWindow.xaml. Prompts user to create a backup before updating.
    /// </summary>
    public partial class UpdateDetectedWindow : Window
    {
        /// <summary>
        /// Gets whether the user wants to create a backup before updating.
        /// </summary>
        public bool UserWantsBackup { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateDetectedWindow"/> class.
        /// </summary>
        /// <param name="owner">The owner window.</param>
        /// <param name="message">The update message to display.</param>
        public UpdateDetectedWindow(Window owner, string message)
        {
            InitializeComponent();
            Owner = owner;
            
            // Update localized texts
            UpdateLocalizedTexts(message);
            
            // Ensure texts are updated after window is loaded
            Loaded += (s, e) => UpdateLocalizedTexts(message);
        }
        
        private void UpdateLocalizedTexts(string message)
        {
            Title = LocalizationHelper.GetString("UpdateDetected");
            TitleText.Text = LocalizationHelper.GetString("UpdateDetected");
            MessageText.Text = message;
            CreateBackupButton.Content = LocalizationHelper.GetString("CreateBackup");
            CloseButton.Content = LocalizationHelper.GetString("Close");
        }

        private void CreateBackup_Click(object sender, RoutedEventArgs e)
        {
            UserWantsBackup = true;
            DialogResult = true;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            UserWantsBackup = false;
            DialogResult = false;
            Close();
        }
    }
}

