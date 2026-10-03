using System.CommandLine;
using System.Text.Json;
using IECODE.Core;
using IECODE.Core.Native;

namespace IECODE.Cli.Commands;

public static class VfsCommand
{
    public static Command Create(Option<string?> gameOption)
    {
        var command = new Command("vfs", "Read the installed game through the native NIE virtual file system");
        var stats = new Command("stats", "Count indexed virtual files");
        var statsJson = new Option<bool>("--json", "Output JSON");
        stats.AddOption(statsJson);
        stats.SetHandler((string? game, bool json) => Run(game, vfs =>
        {
            if (json) Console.WriteLine($"{{\"entry_count\":{vfs.Count}}}");
            else Console.WriteLine($"Virtual files: {vfs.Count}");
        }), gameOption, statsJson);
        command.AddCommand(stats);
        var find = new Command("find", "Find virtual paths by case-insensitive substring");
        var pattern = new Argument<string>("pattern");
        var limit = new Option<int>("--limit", () => 20, "Maximum results (1–10000)");
        var findJson = new Option<bool>("--json", "Output JSON");
        find.AddArgument(pattern); find.AddOption(limit); find.AddOption(findJson);
        find.SetHandler((string? game, string text, int maximum, bool json) => Run(game, vfs =>
        {
            if (maximum < 1 || maximum > 10000) throw new ArgumentOutOfRangeException(nameof(maximum), "Limit must be between 1 and 10000.");
            var entries = vfs.Entries().Where(e => (e.GetProperty("path").GetString() ?? "").Contains(text, StringComparison.OrdinalIgnoreCase)).Take(maximum);
            if (json) Console.WriteLine("[" + string.Join(",", entries.Select(e => e.GetRawText())) + "]");
            else foreach (var entry in entries) Console.WriteLine(entry.GetProperty("path").GetString());
        }), gameOption, pattern, limit, findJson);
        command.AddCommand(find);
        var decode = new Command("decode", "Decode a virtual file using the maintained native format registry");
        var path = new Argument<string>("virtual-path");
        decode.AddArgument(path);
        decode.AddOption(new Option<bool>("--json", "Output JSON (default)"));
        decode.SetHandler((string? game, string virtualPath) => Run(game, vfs => Console.WriteLine(vfs.Decode(vfs.Read(virtualPath)))), gameOption, path);
        command.AddCommand(decode);
        return command;
    }

    private static void Run(string? path, Action<NativeGameVfs> action)
    {
        try { using var vfs = new NativeGameVfs(path ?? IEVRGame.ResolveDefaultGamePath()); action(vfs); }
        catch (Exception error) { Console.Error.WriteLine($"Error: {error.Message}"); Environment.ExitCode = 1; }
    }
}
