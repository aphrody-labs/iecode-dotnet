using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using ndsSharp.Viewer.Shared.Services;

namespace ndsSharp.Viewer.Shared.Extensions;

public static class ApplicationExtensions
{
    public static void CopyToClipboard(string text)
    {
        TaskService.Run(async () =>
        {
            if (GetClipboard() is { } clipboard)
            {
                await clipboard.SetTextAsync(text);
            }
        });
    }
    
    public static IClipboard? GetClipboard()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window })
        {
            return window.Clipboard!;
        }

        return null;
    }
}
