using System.Windows.Input;
using Avalonia.Input;

namespace IECODE.Gui.Shell;

/// <summary>One host-owned action shared by menus, toolbars and plugin surfaces.</summary>
public sealed record ShellCommand(
    string Id,
    string Label,
    string AutomationName,
    ICommand Command,
    KeyGesture? Gesture = null);
