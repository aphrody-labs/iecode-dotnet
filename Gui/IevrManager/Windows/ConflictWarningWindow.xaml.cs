using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using IEVRModManager.Helpers;

namespace IEVRModManager.Windows
{
    /// <summary>
    /// Interaction logic for ConflictWarningWindow.xaml. Warns users about file conflicts between mods.
    /// </summary>
    public partial class ConflictWarningWindow : Window
    {
        /// <summary>
        /// Gets whether the user chose to continue despite conflicts.
        /// </summary>
        public bool UserChoseContinue { get; private set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictWarningWindow"/> class.
        /// </summary>
        /// <param name="parent">The parent window.</param>
        /// <param name="conflicts">Dictionary mapping file paths to lists of mod names that conflict on that file.</param>
        public ConflictWarningWindow(Window parent, Dictionary<string, List<string>> conflicts)
        {
            InitializeComponent();
            Owner = parent;

            // Update localized texts
            Title = LocalizationHelper.GetString("FileConflictsDetected");
            TitleText.Text = LocalizationHelper.GetString("FileConflictsDetected");
            WarningMessage1.Text = LocalizationHelper.GetString("ConflictWarningMessage1");
            WarningMessage2.Text = LocalizationHelper.GetString("ConflictWarningMessage2");
            WarningMessage3.Text = LocalizationHelper.GetString("ConflictWarningMessage3");
            CancelButton.Content = LocalizationHelper.GetString("Cancel");
            ContinueButton.Content = LocalizationHelper.GetString("Continue");

            // Create a list of anonymous objects to display in the ItemsControl
            var conflictItems = conflicts.Select(kvp => new
            {
                Key = kvp.Key,
                Value = $"Mods: {string.Join(", ", kvp.Value)}"
            }).ToList();

            ConflictsListControl.ItemsSource = conflictItems;

            // Close with Escape key
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    Cancel_Click(null!, null!);
                }
            };

            Loaded += (s, e) =>
            {
                CancelButton.Focus();
            };
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