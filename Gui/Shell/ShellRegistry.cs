using System.Collections.ObjectModel;

namespace IECODE.Gui.Shell;

/// <summary>
/// Host-owned registry. It gives every GUI surface one command and navigation identity while
/// allowing the absorbed plugin API to remain an adapter during migration.
/// </summary>
public sealed class ShellRegistry
{
    private readonly Dictionary<string, ShellCommand> _commands = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ShellContribution> _contributions = new(StringComparer.Ordinal);

    public static ShellRegistry Current { get; } = new();

    public ReadOnlyDictionary<string, ShellCommand> Commands => new(_commands);
    public ReadOnlyDictionary<string, ShellContribution> Contributions => new(_contributions);

    public void Register(ShellContribution contribution)
    {
        ArgumentException.ThrowIfNullOrEmpty(contribution.PluginId);
        if (!_contributions.TryAdd(contribution.PluginId, contribution))
        {
            throw new InvalidOperationException($"A shell contribution already exists: {contribution.PluginId}");
        }

        foreach (var command in contribution.Commands)
        {
            if (!_commands.TryAdd(command.Id, command))
            {
                foreach (var registeredCommand in contribution.Commands)
                {
                    _commands.Remove(registeredCommand.Id);
                }
                _contributions.Remove(contribution.PluginId);
                throw new InvalidOperationException($"A shell command already exists: {command.Id}");
            }
        }
    }

    public bool TryGetCommand(string id, out ShellCommand? command) => _commands.TryGetValue(id, out command);
}
