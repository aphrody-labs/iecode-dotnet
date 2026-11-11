using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace IECODE.Gui.Shell;

/// <summary>A minimal functional Avalonia view for the IEVR shell contribution.</summary>
public partial class IevrShellView : UserControl
{
    public IevrShellView()
    {
        InitializeComponent();

        if (!Design.IsDesignMode)
        {
            DataContext = new IevrShellViewModel();
        }
    }

    public IevrShellView(IevrShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
