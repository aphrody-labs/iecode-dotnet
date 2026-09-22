using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using IEVRModManager.Helpers;

namespace IEVRModManager.Windows
{
    /// <summary>
    /// Interaction logic for SuccessMessageWindow.xaml. Displays a success message, optionally with a list of applied mods.
    /// </summary>
    public partial class SuccessMessageWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SuccessMessageWindow"/> class.
        /// </summary>
        /// <param name="parent">The parent window.</param>
        /// <param name="message">The success message to display.</param>
        /// <param name="modNames">Optional list of mod names that were applied.</param>
        public SuccessMessageWindow(Window parent, string message, List<string>? modNames = null)
        {
            InitializeComponent();
            Owner = parent;
            
            // Update localized texts
            Title = LocalizationHelper.GetString("ModsApplied");
            TitleText.Text = LocalizationHelper.GetString("ModsAppliedSuccessfully");
            MessageTextBlock.Text = message;
            OKButton.Content = LocalizationHelper.GetString("OK");
            
            // Show mod list if provided
            if (modNames != null && modNames.Any())
            {
                ModsListControl.ItemsSource = modNames;
            }
            else
            {
                // Hide list if there are no mods
                ModsListControl.Visibility = Visibility.Collapsed;
            }
            
            // Close with Enter or Escape key
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter || e.Key == Key.Escape)
                {
                    Close();
                }
            };
            
            Loaded += (s, e) => Focus();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

