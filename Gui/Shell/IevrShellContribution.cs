namespace IECODE.Gui.Shell;

/// <summary>Registers the IEVR surface with the existing Avalonia shell.</summary>
public static class IevrShellContribution
{
    public const string PluginId = "ievr";

    public static ShellContribution Create()
    {
        var viewModel = new IevrShellViewModel();
        var contribution = new ShellContribution(PluginId);

        contribution.Commands.Add(new ShellCommand(
            "ievr.refresh",
            "Refresh IEVR",
            "ievr-refresh",
            viewModel.RefreshCommand));
        contribution.Commands.Add(new ShellCommand(
            "ievr.detect-steam",
            "Detect Steam game",
            "ievr-detect-steam",
            viewModel.DetectSteamCommand));
        contribution.Commands.Add(new ShellCommand(
            "ievr.save-configuration",
            "Save IEVR configuration",
            "ievr-save-configuration",
            viewModel.SaveConfigurationCommand));

        contribution.NavigationEntries.Add(new ShellNavigationEntry(
            "ievr",
            "IEVR",
            "ievr-shell",
            () => new IevrShellView(viewModel)));

        return contribution;
    }

    public static ShellContribution Register(ShellRegistry? registry = null)
    {
        var contribution = Create();
        (registry ?? ShellRegistry.Current).Register(contribution);
        return contribution;
    }
}
