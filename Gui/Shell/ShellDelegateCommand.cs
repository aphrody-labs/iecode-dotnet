using System.Windows.Input;

namespace IECODE.Gui.Shell;

/// <summary>A small host-neutral command implementation for shell contributions.</summary>
public sealed class ShellDelegateCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
    private readonly Action _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private readonly Func<bool> _canExecute = canExecute ?? (() => true);

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute();

    public void Execute(object? parameter) => _execute();

    public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
