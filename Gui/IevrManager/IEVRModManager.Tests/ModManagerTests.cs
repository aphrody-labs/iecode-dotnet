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
    public class ModManagerTests : IDisposable
    {
        private readonly string _testModsDir;
        private readonly ModManager _modManager;

        public ModManagerTests()
        {
            _testModsDir = Path.Combine(Path.GetTempPath(), $"ModManagerTests_{Guid.NewGuid()}");
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
        public void ScanMods_EmptyDirectory_ReturnsEmptyList()
        {
            // Act
            var result = _modManager.ScanMods();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void ScanMods_WithModDirectories_ReturnsAllMods()
        {
            // Arrange
            CreateTestMod("Mod1");
            CreateTestMod("Mod2");
            CreateTestMod("Mod3");

            // Act
            var result = _modManager.ScanMods();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(result, m => m.Name == "Mod1");
            Assert.Contains(result, m => m.Name == "Mod2");
            Assert.Contains(result, m => m.Name == "Mod3");
        }

        [Fact]
        public void ScanMods_PreservesEnabledStateFromSavedMods()
        {
            // Arrange
            CreateTestMod("Mod1");
            CreateTestMod("Mod2");
            var savedMods = new List<ModData>
            {
                new ModData { Name = "Mod1", Enabled = false },
                new ModData { Name = "Mod2", Enabled = true }
            };

            // Act
            var result = _modManager.ScanMods(savedMods);

            // Assert
            Assert.False(result.First(m => m.Name == "Mod1").Enabled);
            Assert.True(result.First(m => m.Name == "Mod2").Enabled);
        }

        [Fact]
        public void ScanMods_PreservesOrderFromSavedMods()
        {
            // Arrange
            CreateTestMod("Mod1");
            CreateTestMod("Mod2");
            CreateTestMod("Mod3");
            var savedMods = new List<ModData>
            {
                new ModData { Name = "Mod3", Enabled = true },
                new ModData { Name = "Mod1", Enabled = true },
                new ModData { Name = "Mod2", Enabled = true }
            };

            // Act
            var result = _modManager.ScanMods(savedMods);

            // Assert
            Assert.Equal("Mod3", result[0].Name);
            Assert.Equal("Mod1", result[1].Name);
            Assert.Equal("Mod2", result[2].Name);
        }

        [Fact]
        public void ScanMods_LoadsModMetadata_FromModDataJson()
        {
            // Arrange
            var modPath = Path.Combine(_testModsDir, "TestMod");
            Directory.CreateDirectory(modPath);
            var modDataPath = Path.Combine(modPath, "mod_data.json");
            var modData = new Dictionary<string, object>
            {
                { "Name", "Test Mod Display Name" },
                { "Author", "Test Author" },
                { "ModVersion", "1.0.0" },
                { "GameVersion", "1.5.1" },
                { "ModLink", "https://example.com/mod" }
            };
            File.WriteAllText(modDataPath, JsonSerializer.Serialize(modData));

            // Act
            var result = _modManager.ScanMods();

            // Assert
            var mod = result.First();
            Assert.Equal("Test Mod Display Name", mod.DisplayName);
            Assert.Equal("Test Author", mod.Author);
            Assert.Equal("1.0.0", mod.ModVersion);
            Assert.Equal("1.5.1", mod.GameVersion);
            Assert.Equal("https://example.com/mod", mod.ModLink);
        }

        [Fact]
        public void ScanMods_WithoutModDataJson_UsesDirectoryNameAsDisplayName()
        {
            // Arrange
            CreateTestMod("TestMod");

            // Act
            var result = _modManager.ScanMods();

            // Assert
            var mod = result.First();
            Assert.Equal("TestMod", mod.DisplayName);
        }

        [Fact]
        public void ScanMods_ExistingEntriesTakePriorityOverSavedMods()
        {
            // Arrange
            CreateTestMod("Mod1");
            var savedMods = new List<ModData>
            {
                new ModData { Name = "Mod1", Enabled = false }
            };
            var existingEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", _testModsDir, enabled: true)
            };

            // Act
            var result = _modManager.ScanMods(savedMods, existingEntries);

            // Assert
            Assert.True(result.First(m => m.Name == "Mod1").Enabled);
        }

        [Fact]
        public void GetEnabledMods_ReturnsOnlyEnabledMods()
        {
            // Arrange
            var modEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", _testModsDir, enabled: true),
                new ModEntry("Mod2", _testModsDir, enabled: false),
                new ModEntry("Mod3", _testModsDir, enabled: true)
            };

            // Act
            var result = _modManager.GetEnabledMods(modEntries);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(Path.Combine(_testModsDir, "Mod1"), result);
            Assert.Contains(Path.Combine(_testModsDir, "Mod3"), result);
            Assert.DoesNotContain(Path.Combine(_testModsDir, "Mod2"), result);
        }

        [Fact]
        public void DetectFileConflicts_NoConflicts_ReturnsEmptyDictionary()
        {
            // Arrange
            var mod1Path = Path.Combine(_testModsDir, "Mod1");
            var mod2Path = Path.Combine(_testModsDir, "Mod2");
            Directory.CreateDirectory(Path.Combine(mod1Path, "data", "textures"));
            Directory.CreateDirectory(Path.Combine(mod2Path, "data", "models"));
            File.WriteAllText(Path.Combine(mod1Path, "data", "textures", "tex1.dds"), "data");
            File.WriteAllText(Path.Combine(mod2Path, "data", "models", "model1.obj"), "data");

            var modEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", _testModsDir, enabled: true),
                new ModEntry("Mod2", _testModsDir, enabled: true)
            };

            // Act
            var result = _modManager.DetectFileConflicts(modEntries);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void DetectFileConflicts_WithConflicts_ReturnsConflicts()
        {
            // Arrange
            var mod1Path = Path.Combine(_testModsDir, "Mod1");
            var mod2Path = Path.Combine(_testModsDir, "Mod2");
            Directory.CreateDirectory(Path.Combine(mod1Path, "data"));
            Directory.CreateDirectory(Path.Combine(mod2Path, "data"));
            File.WriteAllText(Path.Combine(mod1Path, "data", "conflict.txt"), "mod1");
            File.WriteAllText(Path.Combine(mod2Path, "data", "conflict.txt"), "mod2");

            var modEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", _testModsDir, enabled: true, displayName: "Mod 1"),
                new ModEntry("Mod2", _testModsDir, enabled: true, displayName: "Mod 2")
            };

            // Act
            var result = _modManager.DetectFileConflicts(modEntries);

            // Assert
            Assert.Single(result);
            Assert.True(result.ContainsKey("conflict.txt"));
            Assert.Equal(2, result["conflict.txt"].Count);
            Assert.Contains("Mod 1", result["conflict.txt"]);
            Assert.Contains("Mod 2", result["conflict.txt"]);
        }

        [Fact]
        public void DetectFileConflicts_IgnoresCpkListCfgBin()
        {
            // Arrange
            var mod1Path = Path.Combine(_testModsDir, "Mod1");
            var mod2Path = Path.Combine(_testModsDir, "Mod2");
            Directory.CreateDirectory(Path.Combine(mod1Path, "data"));
            Directory.CreateDirectory(Path.Combine(mod2Path, "data"));
            File.WriteAllText(Path.Combine(mod1Path, "data", "cpk_list.cfg.bin"), "data1");
            File.WriteAllText(Path.Combine(mod2Path, "data", "cpk_list.cfg.bin"), "data2");

            var modEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", _testModsDir, enabled: true),
                new ModEntry("Mod2", _testModsDir, enabled: true)
            };

            // Act
            var result = _modManager.DetectFileConflicts(modEntries);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void DetectFileConflicts_LessThanTwoEnabledMods_ReturnsEmpty()
        {
            // Arrange
            var modEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", _testModsDir, enabled: true),
                new ModEntry("Mod2", _testModsDir, enabled: false)
            };

            // Act
            var result = _modManager.DetectFileConflicts(modEntries);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void DetectPacksModifiers_ModTouchesPacksFolder_ReturnsModName()
        {
            // Arrange
            var modPath = Path.Combine(_testModsDir, "PacksMod");
            Directory.CreateDirectory(Path.Combine(modPath, "data", "packs", "subfolder"));
            File.WriteAllText(Path.Combine(modPath, "data", "packs", "subfolder", "file.txt"), "data");

            var modEntries = new List<ModEntry>
            {
                new ModEntry("PacksMod", _testModsDir, enabled: true, displayName: "Packs Modifier")
            };

            // Act
            var result = _modManager.DetectPacksModifiers(modEntries);

            // Assert
            Assert.Single(result);
            Assert.Contains("Packs Modifier", result);
        }

        [Fact]
        public void DetectPacksModifiers_ModDoesNotTouchPacksFolder_ReturnsEmpty()
        {
            // Arrange
            var modPath = Path.Combine(_testModsDir, "NormalMod");
            Directory.CreateDirectory(Path.Combine(modPath, "data", "textures"));
            File.WriteAllText(Path.Combine(modPath, "data", "textures", "tex.dds"), "data");

            var modEntries = new List<ModEntry>
            {
                new ModEntry("NormalMod", _testModsDir, enabled: true)
            };

            // Act
            var result = _modManager.DetectPacksModifiers(modEntries);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void DetectPacksModifiers_DisabledMod_NotIncluded()
        {
            // Arrange
            var modPath = Path.Combine(_testModsDir, "PacksMod");
            Directory.CreateDirectory(Path.Combine(modPath, "data", "packs"));
            File.WriteAllText(Path.Combine(modPath, "data", "packs", "file.txt"), "data");

            var modEntries = new List<ModEntry>
            {
                new ModEntry("PacksMod", _testModsDir, enabled: false)
            };

            // Act
            var result = _modManager.DetectPacksModifiers(modEntries);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void DetectPacksModifiers_ModWithoutDataFolder_ReturnsEmpty()
        {
            // Arrange
            var modPath = Path.Combine(_testModsDir, "ModWithoutData");
            Directory.CreateDirectory(modPath);

            var modEntries = new List<ModEntry>
            {
                new ModEntry("ModWithoutData", _testModsDir, enabled: true)
            };

            // Act
            var result = _modManager.DetectPacksModifiers(modEntries);

            // Assert
            Assert.Empty(result);
        }

        private void CreateTestMod(string modName)
        {
            var modPath = Path.Combine(_testModsDir, modName);
            Directory.CreateDirectory(modPath);
        }
    }
}

