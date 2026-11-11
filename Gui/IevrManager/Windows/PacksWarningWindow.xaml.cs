using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using IEVRModManager.Helpers;

namespace IEVRModManager.Windows
{
    /// <summary>
    /// Interaction logic for PacksWarningWindow.xaml. Warns users about mods that modify the packs folder.
    /// </summary>
    public partial class PacksWarningWindow : Window
    {
        /// <summary>
        /// Gets whether the user chose to continue despite the warning.
        /// </summary>
        public bool UserChoseContinue { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PacksWarningWindow"/> class.
        /// </summary>
        /// <param name="parent">The parent window.</param>
        /// <param name="modNames">List of mod names that modify the packs folder.</param>
        public PacksWarningWindow(Window parent, List<string> modNames)
        {
            InitializeComponent();
            Owner = parent;

            // Update localized texts
            Title = Helpers.LocalizationHelper.GetString("DataPacksWarning");
            TitleText.Text = Helpers.LocalizationHelper.GetString("WarningDataPacks");
            WarningMessage1.Text = Helpers.LocalizationHelper.GetString("PacksWarningMessage1");
            WarningMessage2.Text = Helpers.LocalizationHelper.GetString("PacksWarningMessage2");
            WarningMessage3.Text = Helpers.LocalizationHelper.GetString("PacksWarningMessage3");
            WarningMessage4.Text = Helpers.LocalizationHelper.GetString("PacksWarningMessage4");
            CancelButton.Content = Helpers.LocalizationHelper.GetString("Cancel");
            ContinueButton.Content = Helpers.LocalizationHelper.GetString("Continue");

            ModsListControl.ItemsSource = modNames?.Where(n => !string.IsNullOrWhiteSpace(n)).ToList() ?? new List<string>();

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
