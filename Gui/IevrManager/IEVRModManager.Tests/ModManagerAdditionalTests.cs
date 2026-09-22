using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using IEVRModManager.Managers;
using IEVRModManager.Models;
using Xunit;

namespace IEVRModManager.Tests
{
    public class ModManagerAdditionalTests : IDisposable
    {
        private readonly string _testModsDir;
        private readonly ModManager _modManager;

        public ModManagerAdditionalTests()
        {
            _testModsDir = Path.Combine(Path.GetTempPath(), $"ModManagerAdditionalTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testModsDir);
            _modManager = new ModManager(_testModsDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testModsDir))
            {
                Directory.Delete(_testModsDir, true);
            }
        }

        [Fact]
        public void ScanMods_ModWithSpecialCharactersInName_HandlesCorrectly()
        {
            // Arrange
            var modName = "Mod-With_Special.Chars";
            CreateTestMod(modName);

            // Act
            var result = _modManager.ScanMods();

            // Assert
            Assert.Single(result);
            Assert.Equal(modName, result[0].Name);
        }

        [Fact]
        public void ScanMods_ModWithVeryLongName_HandlesCorrectly()
        {
            // Arrange
            var longName = new string('A', 200);
            CreateTestMod(longName);

            // Act
            var result = _modManager.ScanMods();

            // Assert
            Assert.Single(result);
            Assert.Equal(longName, result[0].Name);
        }

        [Fact]
        public void ScanMods_ModDataJsonWithMissingFields_UsesDefaults()
        {
            // Arrange
            var modPath = Path.Combine(_testModsDir, "PartialMod");
            Directory.CreateDirectory(modPath);
            var modDataPath = Path.Combine(modPath, "mod_data.json");
            var modData = new Dictionary<string, object>
            {
                { "Name", "Partial Mod" }
                // Missing Author, ModVersion, etc.
            };
            File.WriteAllText(modDataPath, JsonSerializer.Serialize(modData));

            // Act
            var result = _modManager.ScanMods();

            // Assert
            var mod = result.First();
            Assert.Equal("Partial Mod", mod.DisplayName);
            Assert.Equal(string.Empty, mod.Author);
            Assert.Equal(string.Empty, mod.ModVersion);
        }

        [Fact]
        public void ScanMods_ModDataJsonWithInvalidJson_UsesDirectoryName()
        {
            // Arrange
            var modPath = Path.Combine(_testModsDir, "InvalidJsonMod");
            Directory.CreateDirectory(modPath);
            var modDataPath = Path.Combine(modPath, "mod_data.json");
            File.WriteAllText(modDataPath, "{ invalid json }");

            // Act
            var result = _modManager.ScanMods();

            // Assert
            var mod = result.First();
            Assert.Equal("InvalidJsonMod", mod.DisplayName);
        }

        [Fact]
        public void ScanMods_ModDataJsonWithNullValues_HandlesCorrectly()
        {
            // Arrange
            var modPath = Path.Combine(_testModsDir, "NullValuesMod");
            Directory.CreateDirectory(modPath);
            var modDataPath = Path.Combine(modPath, "mod_data.json");
            var modData = new Dictionary<string, object?>
            {
                { "Name", null },
                { "Author", null }
            };
            File.WriteAllText(modDataPath, JsonSerializer.Serialize(modData));

            // Act
            var result = _modManager.ScanMods();

            // Assert
            var mod = result.First();
            Assert.Equal("NullValuesMod", mod.DisplayName); // Should fallback to directory name
        }

        [Fact]
        public void ScanMods_NewModsAddedAfterSavedConfig_AppendedToEnd()
        {
            // Arrange
            CreateTestMod("Mod1");
            CreateTestMod("Mod2");
            var savedMods = new List<ModData>
            {
                new ModData { Name = "Mod1", Enabled = true }
            };

            // Act
            var result = _modManager.ScanMods(savedMods);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Mod1", result[0].Name);
            Assert.Equal("Mod2", result[1].Name);
        }

        [Fact]
        public void ScanMods_RemovedModsFromSavedConfig_NotIncluded()
        {
            // Arrange
            CreateTestMod("Mod1");
            var savedMods = new List<ModData>
            {
                new ModData { Name = "Mod1", Enabled = true },
                new ModData { Name = "NonExistentMod", Enabled = true }
            };

            // Act
            var result = _modManager.ScanMods(savedMods);

            // Assert
            Assert.Single(result);
            Assert.Equal("Mod1", result[0].Name);
            Assert.DoesNotContain(result, m => m.Name == "NonExistentMod");
        }

        [Fact]
        public void DetectFileConflicts_MultipleConflicts_ReturnsAllConflicts()
        {
            // Arrange
            var mod1Path = Path.Combine(_testModsDir, "Mod1");
            var mod2Path = Path.Combine(_testModsDir, "Mod2");
            Directory.CreateDirectory(Path.Combine(mod1Path, "data"));
            Directory.CreateDirectory(Path.Combine(mod2Path, "data"));
            File.WriteAllText(Path.Combine(mod1Path, "data", "conflict1.txt"), "data1");
            File.WriteAllText(Path.Combine(mod2Path, "data", "conflict1.txt"), "data2");
            File.WriteAllText(Path.Combine(mod1Path, "data", "conflict2.txt"), "data1");
            File.WriteAllText(Path.Combine(mod2Path, "data", "conflict2.txt"), "data2");

            var modEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", _testModsDir, enabled: true, displayName: "Mod 1"),
                new ModEntry("Mod2", _testModsDir, enabled: true, displayName: "Mod 2")
            };

            // Act
            var result = _modManager.DetectFileConflicts(modEntries);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey("conflict1.txt"));
            Assert.True(result.ContainsKey("conflict2.txt"));
        }

        [Fact]
        public void DetectFileConflicts_ConflictsInSubdirectories_DetectsCorrectly()
        {
            // Arrange
            var mod1Path = Path.Combine(_testModsDir, "Mod1");
            var mod2Path = Path.Combine(_testModsDir, "Mod2");
            Directory.CreateDirectory(Path.Combine(mod1Path, "data", "subfolder"));
            Directory.CreateDirectory(Path.Combine(mod2Path, "data", "subfolder"));
            File.WriteAllText(Path.Combine(mod1Path, "data", "subfolder", "file.txt"), "data1");
            File.WriteAllText(Path.Combine(mod2Path, "data", "subfolder", "file.txt"), "data2");

            var modEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", _testModsDir, enabled: true, displayName: "Mod 1"),
                new ModEntry("Mod2", _testModsDir, enabled: true, displayName: "Mod 2")
            };

            // Act
            var result = _modManager.DetectFileConflicts(modEntries);

            // Assert
            Assert.Single(result);
            Assert.True(result.ContainsKey("subfolder/file.txt"));
        }

        [Fact]
        public void DetectFileConflicts_ThreeModsWithSameConflict_AllThreeIncluded()
        {
            // Arrange
            var mod1Path = Path.Combine(_testModsDir, "Mod1");
            var mod2Path = Path.Combine(_testModsDir, "Mod2");
            var mod3Path = Path.Combine(_testModsDir, "Mod3");
            Directory.CreateDirectory(Path.Combine(mod1Path, "data"));
            Directory.CreateDirectory(Path.Combine(mod2Path, "data"));
            Directory.CreateDirectory(Path.Combine(mod3Path, "data"));
            File.WriteAllText(Path.Combine(mod1Path, "data", "conflict.txt"), "data1");
            File.WriteAllText(Path.Combine(mod2Path, "data", "conflict.txt"), "data2");
            File.WriteAllText(Path.Combine(mod3Path, "data", "conflict.txt"), "data3");

            var modEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", _testModsDir, enabled: true, displayName: "Mod 1"),
                new ModEntry("Mod2", _testModsDir, enabled: true, displayName: "Mod 2"),
                new ModEntry("Mod3", _testModsDir, enabled: true, displayName: "Mod 3")
            };

            // Act
            var result = _modManager.DetectFileConflicts(modEntries);

            // Assert
            Assert.Single(result);
            Assert.Equal(3, result["conflict.txt"].Count);
            Assert.Contains("Mod 1", result["conflict.txt"]);
            Assert.Contains("Mod 2", result["conflict.txt"]);
            Assert.Contains("Mod 3", result["conflict.txt"]);
        }

        [Fact]
        public void DetectFileConflicts_ConflictsWithSameFileName_DetectsCorrectly()
        {
            // Arrange
            var mod1Path = Path.Combine(_testModsDir, "Mod1");
            var mod2Path = Path.Combine(_testModsDir, "Mod2");
            Directory.CreateDirectory(Path.Combine(mod1Path, "data"));
            Directory.CreateDirectory(Path.Combine(mod2Path, "data"));
            File.WriteAllText(Path.Combine(mod1Path, "data", "file.txt"), "data1");
            File.WriteAllText(Path.Combine(mod2Path, "data", "file.txt"), "data2");

            var modEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", _testModsDir, enabled: true, displayName: "Mod 1"),
                new ModEntry("Mod2", _testModsDir, enabled: true, displayName: "Mod 2")
            };

            // Act
            var result = _modManager.DetectFileConflicts(modEntries);

            // Assert
            Assert.Single(result);
            Assert.True(result.ContainsKey("file.txt"));
            Assert.Equal(2, result["file.txt"].Count);
        }

        [Fact]
        public void DetectPacksModifiers_ModWithPacksInSubdirectory_DetectsCorrectly()
        {
            // Arrange
            var modPath = Path.Combine(_testModsDir, "DeepPacksMod");
            // The code checks if path starts with "packs/", so we need packs directly under data
            Directory.CreateDirectory(Path.Combine(modPath, "data", "packs", "subfolder"));
            File.WriteAllText(Path.Combine(modPath, "data", "packs", "subfolder", "file.txt"), "data");

            var modEntries = new List<ModEntry>
            {
                new ModEntry("DeepPacksMod", _testModsDir, enabled: true, displayName: "Deep Packs Mod")
            };

            // Act
            var result = _modManager.DetectPacksModifiers(modEntries);

            // Assert
            Assert.Single(result);
            Assert.Contains("Deep Packs Mod", result);
        }

        [Fact]
        public void DetectPacksModifiers_ModWithPacksFolderButEmpty_DetectsCorrectly()
        {
            // Arrange
            var modPath = Path.Combine(_testModsDir, "EmptyPacksMod");
            Directory.CreateDirectory(Path.Combine(modPath, "data", "packs"));

            var modEntries = new List<ModEntry>
            {
                new ModEntry("EmptyPacksMod", _testModsDir, enabled: true)
            };

            // Act
            var result = _modManager.DetectPacksModifiers(modEntries);

            // Assert
            Assert.Empty(result); // Empty folder shouldn't be detected
        }

        [Fact]
        public void GetEnabledMods_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            var modEntries = new List<ModEntry>();

            // Act
            var result = _modManager.GetEnabledMods(modEntries);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetEnabledMods_AllDisabled_ReturnsEmptyList()
        {
            // Arrange
            var modEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", _testModsDir, enabled: false),
                new ModEntry("Mod2", _testModsDir, enabled: false)
            };

            // Act
            var result = _modManager.GetEnabledMods(modEntries);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void ScanMods_ExistingEntriesWithNewMods_MaintainsOrder()
        {
            // Arrange
            CreateTestMod("Mod1");
            CreateTestMod("Mod2");
            CreateTestMod("Mod3");
            var existingEntries = new List<ModEntry>
            {
                new ModEntry("Mod2", _testModsDir, enabled: true),
                new ModEntry("Mod1", _testModsDir, enabled: true)
            };

            // Act
            var result = _modManager.ScanMods(null, existingEntries);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("Mod2", result[0].Name);
            Assert.Equal("Mod1", result[1].Name);
            Assert.Equal("Mod3", result[2].Name); // New mod at end
        }

        private void CreateTestMod(string modName)
        {
            var modPath = Path.Combine(_testModsDir, modName);
            Directory.CreateDirectory(modPath);
        }
    }
}
