using System.Windows;
using IEVRModManager.Helpers;

namespace IEVRModManager.Windows
{
    /// <summary>
    /// Interaction logic for PendingChangesWindow.xaml. Warns users about pending changes that haven't been applied.
    /// </summary>
    public partial class PendingChangesWindow : Window
    {
        /// <summary>
        /// Gets whether the user chose to continue despite pending changes.
        /// </summary>
        public bool UserChoseContinue { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PendingChangesWindow"/> class.
        /// </summary>
        /// <param name="owner">The owner window.</param>
        /// <param name="message">The message to display.</param>
        public PendingChangesWindow(Window owner, string message)
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
            Title = LocalizationHelper.GetString("ModsNotApplied");
            TitleText.Text = LocalizationHelper.GetString("PendingChangesDetected");
            MessageText.Text = message;
            ContinueButton.Content = LocalizationHelper.GetString("ContinueAnyway");
            CancelButton.Content = LocalizationHelper.GetString("Cancel");
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            UserChoseContinue = true;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            UserChoseContinue = false;
            DialogResult = false;
            Close();
        }
    }
}
