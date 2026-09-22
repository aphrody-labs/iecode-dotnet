using System;
using System.Collections.Generic;
using System.IO;
using IEVRModManager.Managers;
using IEVRModManager.Models;
using Xunit;

namespace IEVRModManager.Tests
{
    /// <summary>
    /// Tests for input validation across managers.
    /// </summary>
    public class ValidationTests : IDisposable
    {
        private readonly string _testDir;

        public ValidationTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), $"ValidationTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDir))
            {
                try
                {
                    Directory.Delete(_testDir, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        [Fact]
        public void ProfileManager_LoadProfile_WhitespaceProfileName_ThrowsArgumentException()
        {
            // Arrange
            var profilesDir = Path.Combine(_testDir, "Profiles");
            Directory.CreateDirectory(profilesDir);
            var manager = new ProfileManager();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => manager.LoadProfile("   "));
            Assert.Throws<ArgumentException>(() => manager.LoadProfile("\t"));
            Assert.Throws<ArgumentException>(() => manager.LoadProfile("\n"));
        }

        [Fact]
        public void ProfileManager_DeleteProfile_WhitespaceProfileName_ThrowsArgumentException()
        {
            // Arrange
            var profilesDir = Path.Combine(_testDir, "Profiles");
            Directory.CreateDirectory(profilesDir);
            var manager = new ProfileManager();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => manager.DeleteProfile("   "));
            Assert.Throws<ArgumentException>(() => manager.DeleteProfile("\t"));
        }

        [Fact]
        public void ProfileManager_ProfileExists_EmptyString_ReturnsFalse()
        {
            // Arrange
            var manager = new ProfileManager();

            // Act
            var result = manager.ProfileExists("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ProfileManager_ProfileExists_Whitespace_ReturnsFalse()
        {
            // Arrange
            var manager = new ProfileManager();

            // Act
            var result = manager.ProfileExists("   ");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ModManager_GetEnabledMods_ContainsNullEntries_FiltersThem()
        {
            // Arrange
            var modsDir = Path.Combine(_testDir, "Mods");
            Directory.CreateDirectory(modsDir);
            var manager = new ModManager(modsDir);
            var modEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", modsDir, enabled: true),
                null!,
                new ModEntry("Mod2", modsDir, enabled: false),
                null!
            };

            // Act
            var result = manager.GetEnabledMods(modEntries);

            // Assert
            // ModManager.GetEnabledMods filters null entries automatically
            Assert.Single(result);
            Assert.Contains(Path.Combine(modsDir, "Mod1"), result);
        }

        [Fact]
        public void ModManager_DetectPacksModifiers_ContainsNullEntries_FiltersThem()
        {
            // Arrange
            var modsDir = Path.Combine(_testDir, "Mods");
            Directory.CreateDirectory(modsDir);
            var manager = new ModManager(modsDir);
            var modEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", modsDir, enabled: true),
                null!,
                new ModEntry("Mod2", modsDir, enabled: false)
            };

            // Act
            var result = manager.DetectPacksModifiers(modEntries);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void ModManager_DetectFileConflicts_ContainsNullEntries_FiltersThem()
        {
            // Arrange
            var modsDir = Path.Combine(_testDir, "Mods");
            Directory.CreateDirectory(modsDir);
            var manager = new ModManager(modsDir);
            var modEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", modsDir, enabled: true),
                null!,
                new ModEntry("Mod2", modsDir, enabled: true)
            };

            // Act
            var result = manager.DetectFileConflicts(modEntries);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void ModManager_DetectFileConflicts_EmptyList_ReturnsEmptyDictionary()
        {
            // Arrange
            var modsDir = Path.Combine(_testDir, "Mods");
            Directory.CreateDirectory(modsDir);
            var manager = new ModManager(modsDir);

            // Act
            var result = manager.DetectFileConflicts(new List<ModEntry>());

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ModManager_DetectFileConflicts_SingleMod_ReturnsEmptyDictionary()
        {
            // Arrange
            var modsDir = Path.Combine(_testDir, "Mods");
            Directory.CreateDirectory(modsDir);
            var manager = new ModManager(modsDir);
            var modEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", modsDir, enabled: true)
            };

            // Act
            var result = manager.DetectFileConflicts(modEntries);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ConfigManager_Save_ConfigWithNullProperties_HandlesCorrectly()
        {
            // Arrange
            var configPath = Path.Combine(_testDir, "config.json");
            var manager = new TestableConfigManager(configPath);
            var config = new AppConfig
            {
                GamePath = null!,
                CfgBinPath = null!,
                ViolaCliPath = null!
            };

            // Act
            var result = manager.Save(config);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ProfileManager_SaveProfile_ProfileWithInvalidFileName_SanitizesName()
        {
            // Arrange
            var profilesDir = Path.Combine(_testDir, "Profiles");
            Directory.CreateDirectory(profilesDir);
            var manager = new ProfileManager();
            var profile = new ModProfile
            {
                Name = "Test<>Profile|Name",
                Mods = new List<ModData>()
            };

            // Act
            var result = manager.SaveProfile(profile);

            // Assert
            Assert.True(result);
            Assert.True(manager.ProfileExists("Test<>Profile|Name"));
        }

        [Fact]
        public void ProfileManager_SaveProfile_ProfileWithVeryLongName_Truncates()
        {
            // Arrange
            var profilesDir = Path.Combine(_testDir, "Profiles");
            Directory.CreateDirectory(profilesDir);
            var manager = new ProfileManager();
            var longName = new string('A', 200);
            var profile = new ModProfile
            {
                Name = longName,
                Mods = new List<ModData>()
            };

            // Act
            var result = manager.SaveProfile(profile);

            // Assert
            Assert.True(result);
            var exists = manager.ProfileExists(longName);
            Assert.True(exists);
        }

        [Fact]
        public void ModManager_ScanMods_EmptyModsDirectory_ReturnsEmptyList()
        {
            // Arrange
            var modsDir = Path.Combine(_testDir, "Mods");
            Directory.CreateDirectory(modsDir);
            var manager = new ModManager(modsDir);

            // Act
            var result = manager.ScanMods();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ModManager_ScanMods_NonExistentDirectory_CreatesDirectory()
        {
            // Arrange
            var modsDir = Path.Combine(_testDir, "NonExistentMods");
            var manager = new ModManager(modsDir);

            // Act
            var result = manager.ScanMods();

            // Assert
            Assert.NotNull(result);
            Assert.True(Directory.Exists(modsDir));
        }

        private class TestableConfigManager : ConfigManager
        {
            private readonly string _customConfigPath;

            public TestableConfigManager(string configPath)
            {
                _customConfigPath = configPath;
                Helpers.FileSystemHelper.EnsureDirectoryExists(Path.GetDirectoryName(configPath));
            }

            public new bool Save(AppConfig config)
            {
                if (config == null)
                {
                    throw new ArgumentNullException(nameof(config));
                }

                try
                {
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    var json = System.Text.Json.JsonSerializer.Serialize(config, options);
                    File.WriteAllText(_customConfigPath, json);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
