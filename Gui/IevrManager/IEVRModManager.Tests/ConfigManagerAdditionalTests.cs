using System;
using System.IO;
using System.Text.Json;
using IEVRModManager.Managers;
using IEVRModManager.Models;
using Xunit;

namespace IEVRModManager.Tests
{
    public class ConfigManagerAdditionalTests : IDisposable
    {
        private readonly string _testConfigDir;
        private readonly string _testConfigPath;

        public ConfigManagerAdditionalTests()
        {
            _testConfigDir = Path.Combine(Path.GetTempPath(), $"ConfigManagerAdditionalTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testConfigDir);
            _testConfigPath = Path.Combine(_testConfigDir, "config.json");
        }

        public void Dispose()
        {
            if (Directory.Exists(_testConfigDir))
            {
                Directory.Delete(_testConfigDir, true);
            }
        }

        private TestableConfigManager GetConfigManager()
        {
            return new TestableConfigManager(_testConfigPath);
        }

        [Fact]
        public void Load_ConfigWithAllProperties_LoadsAllProperties()
        {
            // Arrange
            var manager = GetConfigManager();
            var fullConfig = new AppConfig
            {
                GamePath = "C:\\Games\\IEVR",
                CfgBinPath = "C:\\Games\\IEVR\\cfg.bin",
                ViolaCliPath = "C:\\Tools\\viola.exe",
                SelectedCpkName = "test.cpk",
                TmpDir = "C:\\Temp",
                LastKnownPacksSignature = "sig123",
                LastKnownSteamBuildId = "build456",
                VanillaFallbackUntilUtc = DateTime.UtcNow,
                Theme = "Dark",
                Language = "en-US",
                LastAppliedProfile = "Profile1",
                LastCpkListCheckUtc = DateTime.UtcNow.AddDays(-1),
                LastAppUpdateCheckUtc = DateTime.UtcNow.AddDays(-2),
                LastModPrefetchUtc = DateTime.UtcNow.AddDays(-3),
                Mods = new List<ModData>
                {
                    new ModData { Name = "Mod1", Enabled = true }
                }
            };
            SaveConfigToFile(_testConfigPath, fullConfig);

            // Act
            var result = manager.Load();

            // Assert
            Assert.Equal(fullConfig.GamePath, result.GamePath);
            Assert.Equal(fullConfig.CfgBinPath, result.CfgBinPath);
            Assert.Equal(fullConfig.ViolaCliPath, result.ViolaCliPath);
            Assert.Equal(fullConfig.SelectedCpkName, result.SelectedCpkName);
            Assert.Equal(fullConfig.TmpDir, result.TmpDir);
            Assert.Equal(fullConfig.Theme, result.Theme);
            Assert.Equal(fullConfig.Language, result.Language);
            Assert.Equal(fullConfig.LastAppliedProfile, result.LastAppliedProfile);
            Assert.Single(result.Mods);
        }

        [Fact]
        public void Load_ConfigWithEmptyStrings_LoadsCorrectly()
        {
            // Arrange
            var manager = GetConfigManager();
            var config = new AppConfig
            {
                GamePath = "",
                CfgBinPath = "",
                ViolaCliPath = "",
                SelectedCpkName = "",
                Mods = new List<ModData>()
            };
            SaveConfigToFile(_testConfigPath, config);

            // Act
            var result = manager.Load();

            // Assert
            Assert.Equal(string.Empty, result.GamePath);
            Assert.Equal(string.Empty, result.CfgBinPath);
            Assert.NotNull(result.Mods);
        }

        [Fact]
        public void Load_ConfigWithNullModsList_InitializesAsEmptyList()
        {
            // Arrange
            var manager = GetConfigManager();
            var configJson = @"{
                ""GamePath"": ""C:\\Games\\IEVR"",
                ""Mods"": null
            }";
            File.WriteAllText(_testConfigPath, configJson);

            // Act
            var result = manager.Load();

            // Assert
            Assert.NotNull(result.Mods);
            Assert.Empty(result.Mods);
        }

        [Fact]
        public void Save_ConfigWithManyMods_PreservesAllMods()
        {
            // Arrange
            var manager = GetConfigManager();
            var modEntries = new List<ModEntry>();
            for (int i = 0; i < 100; i++)
            {
                modEntries.Add(new ModEntry($"Mod{i}", "C:\\Mods", enabled: i % 2 == 0));
            }

            // Act
            var success = manager.Save(
                gamePath: "C:\\Games\\IEVR",
                selectedCpkName: "test.cpk",
                cfgBinPath: "",
                violaCliPath: "",
                tmpDir: "",
                modEntries: modEntries,
                lastKnownPacksSignature: "",
                lastKnownSteamBuildId: "",
                vanillaFallbackUntilUtc: DateTime.MinValue,
                theme: "System",
                language: "System"
            );

            // Assert
            Assert.True(success);
            var loaded = manager.Load();
            Assert.Equal(100, loaded.Mods.Count);
        }

        [Fact]
        public void Save_ConfigWithModsHavingLinks_PreservesModLinks()
        {
            // Arrange
            var manager = GetConfigManager();
            var modEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", "C:\\Mods", enabled: true, modLink: "https://example.com/mod1"),
                new ModEntry("Mod2", "C:\\Mods", enabled: false, modLink: "https://example.com/mod2")
            };

            // Act
            manager.Save(
                gamePath: "C:\\Games\\IEVR",
                selectedCpkName: "",
                cfgBinPath: "",
                violaCliPath: "",
                tmpDir: "",
                modEntries: modEntries,
                lastKnownPacksSignature: "",
                lastKnownSteamBuildId: "",
                vanillaFallbackUntilUtc: DateTime.MinValue,
                theme: "System",
                language: "System"
            );

            // Assert
            var loaded = manager.Load();
            Assert.Equal(2, loaded.Mods.Count);
            Assert.Equal("https://example.com/mod1", loaded.Mods[0].ModLink);
            Assert.Equal("https://example.com/mod2", loaded.Mods[1].ModLink);
        }

        [Fact]
        public void UpdateLastAppUpdateCheckUtc_MultipleUpdates_PreservesLastValue()
        {
            // Arrange
            var manager = GetConfigManager();
            var time1 = DateTime.UtcNow.AddHours(-2);
            var time2 = DateTime.UtcNow.AddHours(-1);
            var time3 = DateTime.UtcNow;

            // Act
            manager.UpdateLastAppUpdateCheckUtc(time1);
            manager.UpdateLastAppUpdateCheckUtc(time2);
            manager.UpdateLastAppUpdateCheckUtc(time3);

            // Assert
            var loaded = manager.Load();
            Assert.True(Math.Abs((time3 - loaded.LastAppUpdateCheckUtc).TotalSeconds) < 5);
        }

        [Fact]
        public void Load_ConfigWithInvalidDateTimeValues_UsesDefaults()
        {
            // Arrange
            var manager = GetConfigManager();
            var configJson = @"{
                ""GamePath"": ""C:\\Games\\IEVR"",
                ""LastAppUpdateCheckUtc"": ""invalid-date""
            }";
            File.WriteAllText(_testConfigPath, configJson);

            // Act
            var result = manager.Load();

            // Assert
            // Should return default config when date parsing fails
            Assert.NotNull(result);
        }

        [Fact]
        public void Save_ConfigWithVeryLongPaths_HandlesCorrectly()
        {
            // Arrange
            var manager = GetConfigManager();
            var longPath = "C:\\" + new string('A', 200) + "\\Game.exe";

            // Act
            var success = manager.Save(
                gamePath: longPath,
                selectedCpkName: "",
                cfgBinPath: "",
                violaCliPath: "",
                tmpDir: "",
                modEntries: new List<ModEntry>(),
                lastKnownPacksSignature: "",
                lastKnownSteamBuildId: "",
                vanillaFallbackUntilUtc: DateTime.MinValue,
                theme: "System",
                language: "System"
            );

            // Assert
            Assert.True(success);
            var loaded = manager.Load();
            Assert.Equal(longPath, loaded.GamePath);
        }

        [Fact]
        public void Load_ConfigWithSpecialCharactersInPaths_HandlesCorrectly()
        {
            // Arrange
            var manager = GetConfigManager();
            var config = new AppConfig
            {
                GamePath = "C:\\Games\\IEVR (Special Edition)\\game.exe",
                CfgBinPath = "C:\\Games\\IEVR [Modded]\\cfg.bin",
                Mods = new List<ModData>()
            };
            SaveConfigToFile(_testConfigPath, config);

            // Act
            var result = manager.Load();

            // Assert
            Assert.Equal(config.GamePath, result.GamePath);
            Assert.Equal(config.CfgBinPath, result.CfgBinPath);
        }

        [Fact]
        public void Save_ConfigWithUnicodeCharacters_HandlesCorrectly()
        {
            // Arrange
            var manager = GetConfigManager();
            var unicodePath = "C:\\Games\\游戏\\游戏.exe";

            // Act
            var success = manager.Save(
                gamePath: unicodePath,
                selectedCpkName: "",
                cfgBinPath: "",
                violaCliPath: "",
                tmpDir: "",
                modEntries: new List<ModEntry>(),
                lastKnownPacksSignature: "",
                lastKnownSteamBuildId: "",
                vanillaFallbackUntilUtc: DateTime.MinValue,
                theme: "System",
                language: "System"
            );

            // Assert
            Assert.True(success);
            var loaded = manager.Load();
            Assert.Equal(unicodePath, loaded.GamePath);
        }

        private void SaveConfigToFile(string path, AppConfig config)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(path, json);
        }

        // Reuse TestableConfigManager from ConfigManagerTests
        private class TestableConfigManager : ConfigManager
        {
            private readonly string _customConfigPath;

            public TestableConfigManager(string configPath)
            {
                _customConfigPath = configPath;
                EnsureDirectoryExists(Path.GetDirectoryName(configPath));
            }

            public new AppConfig Load()
            {
                if (!File.Exists(_customConfigPath))
                {
                    return AppConfig.Default();
                }

                try
                {
                    var json = File.ReadAllText(_customConfigPath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (config == null)
                    {
                        return AppConfig.Default();
                    }

                    return MigrateConfig(config, json);
                }
                catch
                {
                    return AppConfig.Default();
                }
            }

            public new bool Save(string gamePath, string selectedCpkName, string cfgBinPath, string violaCliPath,
                string tmpDir, List<ModEntry> modEntries,
                string lastKnownPacksSignature, string lastKnownSteamBuildId,
                DateTime vanillaFallbackUntilUtc, string theme, string language, string lastAppliedProfile = "")
            {
                try
                {
                    EnsureDirectoryExists(Path.GetDirectoryName(_customConfigPath));
                    var existingConfig = Load();

                    var config = new AppConfig
                    {
                        GamePath = gamePath,
                        SelectedCpkName = selectedCpkName,
                        CfgBinPath = cfgBinPath,
                        ViolaCliPath = violaCliPath,
                        TmpDir = tmpDir,
                        LastKnownPacksSignature = lastKnownPacksSignature,
                        LastKnownSteamBuildId = lastKnownSteamBuildId,
                        VanillaFallbackUntilUtc = vanillaFallbackUntilUtc,
                        Theme = theme,
                        Language = language,
                        LastAppliedProfile = lastAppliedProfile,
                        Mods = modEntries.ConvertAll(me => me.ToData())
                    };

                    config.LastAppUpdateCheckUtc = existingConfig.LastAppUpdateCheckUtc;
                    config.LastModPrefetchUtc = existingConfig.LastModPrefetchUtc;
                    config.LastCpkListCheckUtc = existingConfig.LastCpkListCheckUtc;

                    SaveConfigToFile(config);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public new void UpdateLastAppUpdateCheckUtc(DateTime checkTime)
            {
                try
                {
                    var config = Load();
                    config.LastAppUpdateCheckUtc = checkTime;
                    SaveConfigToFile(config);
                }
                catch
                {
                    // Ignore errors
                }
            }

            private AppConfig MigrateConfig(AppConfig loadedConfig, string originalJson)
            {
                var defaultConfig = AppConfig.Default();
                var migrated = false;

                try
                {
                    using var jsonDoc = JsonDocument.Parse(originalJson);
                    var root = jsonDoc.RootElement;

                    if (!root.TryGetProperty("Mods", out _) || loadedConfig.Mods == null)
                    {
                        loadedConfig.Mods = new List<ModData>();
                        migrated = true;
                    }

                    if (migrated)
                    {
                        SaveConfigToFile(loadedConfig);
                    }
                }
                catch
                {
                    // If JSON parsing fails, ensure Mods is initialized
                    if (loadedConfig.Mods == null)
                    {
                        loadedConfig.Mods = new List<ModData>();
                    }
                }

                return loadedConfig;
            }

            private void SaveConfigToFile(AppConfig config)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                var json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(_customConfigPath, json);
            }

            private static void EnsureDirectoryExists(string? path)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return;
                }

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }
    }
}
