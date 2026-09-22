using Avalonia.Controls;

namespace IECODE.Gui.Shell;

/// <summary>A plugin's declarative contribution to the unified shell.</summary>
public sealed class ShellContribution(string pluginId)
{
    public string PluginId { get; } = pluginId;
    public IList<ShellCommand> Commands { get; } = [];
    public IList<ShellNavigationEntry> NavigationEntries { get; } = [];
}

public sealed record ShellNavigationEntry(
    string Id,
    string Label,
    string AutomationName,
    Func<Control> ViewFactory);
