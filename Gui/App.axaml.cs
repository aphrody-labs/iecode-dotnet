using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using IESE.Views;
using IESEViewModel.ViewModels;
using IECODE.Gui.Shell;
using ndsSharp.Viewer.Services;

namespace IESE;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            var mainViewModel = new MainViewModel();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            RegisterCoreCommands(mainViewModel);

            // The NDS viewer is a capability of the unified IECODE GUI, not a
            // second Avalonia application. Keep its services attached to this
            // lifetime so its views and plugins remain usable from the shell.
            ApplicationService.Application = desktop;
            ApplicationService.Initialize();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void RegisterCoreCommands(MainViewModel viewModel)
    {
        var contribution = new ShellContribution("iecode.save-editor");
        contribution.Commands.Add(new ShellCommand(
            "save.open",
            "Open save",
            "Open save file",
            viewModel.OpenSaveCommand,
            new Avalonia.Input.KeyGesture(Avalonia.Input.Key.O, Avalonia.Input.KeyModifiers.Control)));
        contribution.Commands.Add(new ShellCommand(
            "save.info",
            "View information",
            "View save information",
            viewModel.OpenSaveInfoDialogCommand));
        contribution.Commands.Add(new ShellCommand(
            "viewer.nds.open",
            "Open NDS viewer",
            "Open the integrated NDS viewer",
            viewModel.OpenNdsViewerCommand));
        contribution.Commands.Add(new ShellCommand(
            "toolbox.text-decoder.open",
            "Open text decoder",
            "Open the IECODE Toolbox text decoder",
            viewModel.OpenToolboxTextDecoderCommand));
        ShellRegistry.Current.Register(contribution);
        IevrShellContribution.Register();
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
