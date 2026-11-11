using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using IEVRModManager.Models;
using IEVRModManager.Exceptions;

namespace IEVRModManager.Managers
{
    /// <summary>
    /// Manages scanning and detection of mods in the mods directory.
    /// </summary>
    public class ModManager
    {
        private readonly string _modsDir;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModManager"/> class.
        /// </summary>
        /// <param name="modsDir">The directory path where mods are stored. If null, uses the default mods directory.</param>
        public ModManager(string? modsDir = null)
        {
            _modsDir = modsDir ?? Config.DefaultModsDir;
        }

        /// <summary>
        /// Scans the mods directory and returns a list of mod entries.
        /// </summary>
        /// <param name="savedMods">Optional list of saved mod data to restore enabled state.</param>
        /// <param name="existingEntries">Optional list of existing mod entries to preserve order and state.</param>
        /// <returns>A list of <see cref="ModEntry"/> objects representing all mods found in the directory.</returns>
        public List<ModEntry> ScanMods(List<ModData>? savedMods = null, 
            List<ModEntry>? existingEntries = null)
        {
            var modsRoot = Path.GetFullPath(_modsDir);
            Directory.CreateDirectory(modsRoot);

            var modNames = GetModDirectoryNames(modsRoot);
            var enabledStateMap = BuildEnabledStateMap(savedMods, existingEntries);
            var orderedNames = DetermineModOrder(modNames, savedMods, existingEntries);

            return CreateModEntries(orderedNames, modsRoot, enabledStateMap);
        }

        private List<string> GetModDirectoryNames(string modsRoot)
        {
            return Directory.GetDirectories(modsRoot)
                .Select(d => Path.GetFileName(d))
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();
        }

        private Dictionary<string, bool> BuildEnabledStateMap(List<ModData>? savedMods, List<ModEntry>? existingEntries)
        {
            var stateMap = new Dictionary<string, bool>();

            // Priority: existing entries > saved config > default (true)
            if (existingEntries != null)
            {
                foreach (var entry in existingEntries)
                {
                    stateMap[entry.Name] = entry.Enabled;
                }
            }

            if (savedMods != null)
            {
                foreach (var mod in savedMods)
                {
                    if (!stateMap.ContainsKey(mod.Name))
                    {
                        stateMap[mod.Name] = mod.Enabled;
                    }
                }
            }

            return stateMap;
        }

        private List<string> DetermineModOrder(List<string> modNames, List<ModData>? savedMods, List<ModEntry>? existingEntries)
        {
            if (existingEntries != null && existingEntries.Count > 0)
            {
                // Use the current order of existing entries
                var existingOrder = existingEntries
                    .Select(me => me.Name)
                    .Where(modNames.Contains)
                    .ToList();
                
                // Add new mods at the end
                var newMods = modNames
                    .Where(n => !existingOrder.Contains(n))
                    .ToList();
                
                return existingOrder.Concat(newMods).ToList();
            }

            // Fall back to saved config order if no existing entries
            var savedOrder = (savedMods ?? new List<ModData>())
                .Select(m => m.Name)
                .ToList();

            return savedOrder
                .Where(modNames.Contains)
                .Concat(modNames.Where(n => !savedOrder.Contains(n)))
                .ToList();
        }

        private List<ModEntry> CreateModEntries(List<string> orderedNames, string modsRoot, Dictionary<string, bool> enabledStateMap)
        {
            var modEntries = new List<ModEntry>();
            
            foreach (var name in orderedNames)
            {
                var modPath = Path.Combine(modsRoot, name);
                var metadata = LoadModMetadata(modPath);
                var enabled = enabledStateMap.GetValueOrDefault(name, true);

                var modEntry = new ModEntry(
                    name: name,
                    path: modsRoot,
                    enabled: enabled,
                    displayName: metadata.DisplayName,
                    author: metadata.Author,
                    modVersion: metadata.ModVersion,
                    gameVersion: metadata.GameVersion,
                    modLink: metadata.ModLink
                );
                modEntries.Add(modEntry);
            }

            return modEntries;
        }

        private ModMetadata LoadModMetadata(string modPath)
        {
            var metadata = new ModMetadata
            {
                DisplayName = Path.GetFileName(modPath),
                Author = string.Empty,
                ModVersion = string.Empty,
                GameVersion = string.Empty,
                ModLink = string.Empty
            };

            var modDataPath = Path.Combine(modPath, "mod_data.json");
            if (!File.Exists(modDataPath))
            {
                return metadata;
            }

            try
            {
                var json = File.ReadAllText(modDataPath);
                var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                if (data != null)
                {
                    metadata.DisplayName = GetStringValue(data, "Name") ?? metadata.DisplayName;
                    metadata.Author = GetStringValue(data, "Author") ?? string.Empty;
                    metadata.ModVersion = GetStringValue(data, "ModVersion") ?? string.Empty;
                    metadata.GameVersion = GetStringValue(data, "GameVersion") ?? string.Empty;
                    metadata.ModLink = GetStringValue(data, "ModLink") ?? string.Empty;
                }
            }
            catch
            {
                // Ignore errors, return default metadata
            }

            return metadata;
        }

        private static string? GetStringValue(Dictionary<string, JsonElement> data, string key)
        {
            return data.TryGetValue(key, out var element) ? element.GetString() : null;
        }

        /// <summary>
        /// Gets the full paths of all enabled mods from the provided mod entries.
        /// </summary>
        /// <param name="modEntries">The list of mod entries to filter.</param>
        /// <returns>A list of full directory paths for enabled mods.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="modEntries"/> is null.</exception>
        public List<string> GetEnabledMods(List<ModEntry> modEntries)
        {
            if (modEntries == null)
            {
                throw new ArgumentNullException(nameof(modEntries));
            }

            return modEntries
                .Where(me => me != null && me.Enabled)
                .Select(me => me!.FullPath)
                .ToList();
        }

        /// <summary>
        /// Detects mods that modify the packs folder.
        /// </summary>
        /// <param name="modEntries">The list of mod entries to check.</param>
        /// <returns>A list of display names of mods that touch the packs folder.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="modEntries"/> is null.</exception>
        public List<string> DetectPacksModifiers(List<ModEntry> modEntries)
        {
            if (modEntries == null)
            {
                throw new ArgumentNullException(nameof(modEntries));
            }

            var modsTouchingPacks = new List<string>();
            
            foreach (var mod in modEntries.Where(me => me != null && me.Enabled))
            {
                if (ModTouchesPacksFolder(mod!))
                {
                    var displayName = string.IsNullOrWhiteSpace(mod!.DisplayName) ? mod.Name : mod.DisplayName;
                    modsTouchingPacks.Add(displayName);
                }
            }

            return modsTouchingPacks
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private bool ModTouchesPacksFolder(ModEntry mod)
        {
            if (mod == null)
            {
                throw new ArgumentNullException(nameof(mod));
            }

            if (string.IsNullOrWhiteSpace(mod.FullPath))
            {
                return false;
            }

            try
            {
                var dataPath = Path.Combine(mod.FullPath, "data");
                if (!Directory.Exists(dataPath))
                {
                    return false;
                }

                var files = Directory.GetFiles(dataPath, "*", SearchOption.AllDirectories);
                return files.Any(filePath =>
                {
                    var relativePath = Path.GetRelativePath(dataPath, filePath)
                        .Replace('\\', '/');
                    return relativePath.StartsWith("packs/", StringComparison.OrdinalIgnoreCase);
                });
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// Detects file conflicts between enabled mods.
        /// </summary>
        /// <param name="modEntries">The list of mod entries to check for conflicts.</param>
        /// <returns>A dictionary mapping file paths to lists of mod names that conflict on that file.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="modEntries"/> is null.</exception>
        public Dictionary<string, List<string>> DetectFileConflicts(List<ModEntry> modEntries)
        {
            if (modEntries == null)
            {
                throw new ArgumentNullException(nameof(modEntries));
            }

            var conflicts = new Dictionary<string, List<string>>();
            var enabledMods = modEntries.Where(me => me != null && me.Enabled).ToList();

            if (enabledMods.Count < 2)
            {
                return conflicts;
            }

            var fileToModsMap = BuildFileToModsMap(enabledMods!);
            return ExtractConflicts(fileToModsMap);
        }

        private Dictionary<string, HashSet<string>> BuildFileToModsMap(List<ModEntry> enabledMods)
        {
            if (enabledMods == null)
            {
                throw new ArgumentNullException(nameof(enabledMods));
            }

            var fileToModsMap = new Dictionary<string, HashSet<string>>();

            foreach (var mod in enabledMods)
            {
                if (mod == null || string.IsNullOrWhiteSpace(mod.FullPath))
                {
                    continue;
                }

                try
                {
                    var modDataPath = Path.Combine(mod.FullPath, "data");
                    if (!Directory.Exists(modDataPath))
                    {
                        continue;
                    }

                    var files = Directory.GetFiles(modDataPath, "*", SearchOption.AllDirectories);
                    var displayName = string.IsNullOrWhiteSpace(mod.DisplayName) ? mod.Name : mod.DisplayName;
                    
                    foreach (var filePath in files)
                    {
                        var relativePath = Path.GetRelativePath(modDataPath, filePath)
                            .Replace('\\', '/');

                        if (!fileToModsMap.TryGetValue(relativePath, out var modSet))
                        {
                            modSet = new HashSet<string>();
                            fileToModsMap[relativePath] = modSet;
                        }

                        modSet.Add(displayName);
                    }
                }
                catch (IOException)
                {
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
            }

            return fileToModsMap;
        }

        private Dictionary<string, List<string>> ExtractConflicts(Dictionary<string, HashSet<string>> fileToModsMap)
        {
            var conflicts = new Dictionary<string, List<string>>();
            const string cpkListFileName = "cpk_list.cfg.bin";

            foreach (var kvp in fileToModsMap)
            {
                if (kvp.Key.Equals(cpkListFileName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                
                if (kvp.Value.Count > 1)
                {
                    conflicts[kvp.Key] = kvp.Value.OrderBy(m => m).ToList();
                }
            }

            return conflicts;
        }

        private class ModMetadata
        {
            public string DisplayName { get; set; } = string.Empty;
            public string Author { get; set; } = string.Empty;
            public string ModVersion { get; set; } = string.Empty;
            public string GameVersion { get; set; } = string.Empty;
            public string ModLink { get; set; } = string.Empty;
        }
    }
}

