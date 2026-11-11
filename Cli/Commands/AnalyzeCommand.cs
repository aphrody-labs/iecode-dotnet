using IECODE.Core;

namespace IECODE.Cli.Commands;

/// <summary>
/// Commande analyze - Analyse de la structure du jeu.
/// </summary>
public static class AnalyzeCommand
{
    public static async Task ExecuteAsync(string? gamePath, string? output, bool deep, bool verbose)
    {
        try
        {
            string source = IECODE.Core.Native.NativeGameVfs.ResolveSource(gamePath);
            if (IECODE.Core.Native.NativeGameVfs.IsRemoteSource(source))
            {
                using var remote = new IECODE.Core.Native.NativeGameVfs(source);
                var scan = Scan(remote, deep);
                Console.WriteLine($"Remote VFS: {source}\nIndexed virtual files: {remote.Count}\nAnalyzed entries: {scan.Count}");
                foreach (var extension in scan.Extensions.OrderByDescending(e => e.Value).Take(15))
                    Console.WriteLine($"{extension.Key,-15} : {extension.Value,6}");
                if (!string.IsNullOrEmpty(output))
                    await File.WriteAllTextAsync(output, System.Text.Json.JsonSerializer.Serialize(new { source, entry_count = remote.Count, analyzed_entries = scan.Count, limit = deep ? (int?)null : 10000, file_types = scan.Extensions }));
                return;
            }
            using var game = new IEVRGame(source);

            if (!game.IsValid)
            {
                Console.Error.WriteLine($"Error: Game not found at: {game.GamePath}");
                Environment.ExitCode = 1;
                return;
            }

            Console.WriteLine($"Analyzing: {game.GamePath}");
            Console.WriteLine();

            var info = await game.LoadInfoAsync();

            // Basic stats
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    IEVR Game Analysis                        ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            Console.WriteLine("📁 File System:");
            Console.WriteLine($"   CPK Archives: {info.CpkCount}");
            Console.WriteLine($"   Total Size: {info.TotalCpkSizeFormatted}");
            Console.WriteLine();

            // Analyze CPK contents
            Console.WriteLine("📦 CPK Analysis:");

            using var vfs = new IECODE.Core.Native.NativeGameVfs(game.GamePath);
            var (totalFiles, extensions) = Scan(vfs, deep);
            Console.WriteLine($"   Indexed virtual files: {vfs.Count}");
            Console.WriteLine($"   Analyzed virtual entries: {totalFiles}" + (deep ? " (all)" : " (limit 10000; use --deep for all)"));
            foreach (var extension in extensions.OrderByDescending(e => e.Value).Take(15))
                Console.WriteLine($"   {extension.Key,-15} : {extension.Value,6} files");

            // Save JSON report
            if (!string.IsNullOrEmpty(output))
            {
                var report = new GameAnalysisReport
                {
                    GamePath = info.GamePath,
                    PacksPath = game.PacksPath,
                    CpkCount = info.CpkCount,
                    TotalCpkSize = info.TotalCpkSize,
                    AnalyzedAt = DateTime.UtcNow,
                    FileTypes = extensions.OrderByDescending(e => e.Value).ToDictionary(e => e.Key, e => e.Value)
                };

                string json = System.Text.Json.JsonSerializer.Serialize(report, JsonContext.Default.GameAnalysisReport);

                await File.WriteAllTextAsync(output, json);
                Console.WriteLine($"Report saved to: {output}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            if (verbose)
            {
                Console.Error.WriteLine(ex.StackTrace);
            }
            Environment.ExitCode = 1;
        }
    }

    private static (int Count, Dictionary<string, int> Extensions) Scan(IECODE.Core.Native.NativeGameVfs vfs, bool deep)
    {
        int count = 0;
        var extensions = new Dictionary<string, int>();
        foreach (var entry in deep ? vfs.Entries() : vfs.Entries().Take(10000))
        {
            string extension = Path.GetExtension(entry.GetProperty("path").GetString() ?? "").ToLowerInvariant();
            if (extension.Length == 0) extension = "(no ext)";
            extensions[extension] = extensions.GetValueOrDefault(extension) + 1;
            count++;
        }
        return (count, extensions);
    }
}
