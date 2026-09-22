using System.Windows;
using IEVRModManager.Helpers;

namespace IEVRModManager.Windows
{
    /// <summary>
    /// Interaction logic for BackupConfirmationWindow.xaml. Prompts user to confirm backup or restore operations.
    /// </summary>
    public partial class BackupConfirmationWindow : Window
    {
        /// <summary>
        /// Gets whether the user confirmed the operation.
        /// </summary>
        public bool UserConfirmed { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupConfirmationWindow"/> class.
        /// </summary>
        /// <param name="owner">The owner window.</param>
        /// <param name="message">The message to display.</param>
        /// <param name="isRestore">Whether this is a restore operation (true) or backup operation (false).</param>
        public BackupConfirmationWindow(Window owner, string message, bool isRestore = false)
        {
            InitializeComponent();
            Owner = owner;
            
            // Update localized texts
            UpdateLocalizedTexts(message, isRestore);
            
            // Ensure texts are updated after window is loaded
            Loaded += (s, e) => UpdateLocalizedTexts(message, isRestore);
        }
        
        private void UpdateLocalizedTexts(string message, bool isRestore)
        {
            Title = isRestore ? LocalizationHelper.GetString("ConfirmRestoreBackup") : LocalizationHelper.GetString("ConfirmCreateBackup");
            TitleText.Text = isRestore ? LocalizationHelper.GetString("ConfirmRestoreBackup") : LocalizationHelper.GetString("ConfirmCreateBackup");
            MessageText.Text = message;
            ConfirmButton.Content = LocalizationHelper.GetString("Confirm");
            CancelButton.Content = LocalizationHelper.GetString("Cancel");
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            UserConfirmed = true;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            UserConfirmed = false;
            DialogResult = false;
            Close();
        }
    }
}

