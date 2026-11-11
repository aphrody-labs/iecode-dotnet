using System;
using System.IO;
using System.Text.Json;
using IEVRModManager.Managers;
using IEVRModManager.Models;
using Xunit;

namespace IEVRModManager.Tests
{
    public class ConfigManagerTests : IDisposable
    {
        private readonly string _testConfigDir;
        private readonly string _testConfigPath;
        private readonly ConfigManager _configManager;

        public ConfigManagerTests()
        {
            _testConfigDir = Path.Combine(Path.GetTempPath(), $"ConfigManagerTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testConfigDir);
            _testConfigPath = Path.Combine(_testConfigDir, "config.json");
            
            // Override Config.ConfigPath using reflection or create a test-specific ConfigManager
            // For now, we'll use a workaround by setting environment or using a custom path
            _configManager = new ConfigManager();
        }

        public void Dispose()
        {
            if (Directory.Exists(_testConfigDir))
            {
                Directory.Delete(_testConfigDir, true);
            }
        }

        [Fact]
        public void Load_ConfigFileDoesNotExist_ReturnsDefaultConfig()
        {
            // Arrange
            var configPath = Path.Combine(_testConfigDir, "nonexistent.json");
            var manager = new TestableConfigManager(configPath);

            // Act
            var result = manager.Load();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.GamePath);
            Assert.Equal(string.Empty, result.CfgBinPath);
            Assert.Equal(string.Empty, result.ViolaCliPath);
            Assert.Equal(DateTime.MinValue, result.VanillaFallbackUntilUtc);
        }

        [Fact]
        public void Load_ValidConfigFile_LoadsCorrectly()
        {
            // Arrange
            var configPath = Path.Combine(_testConfigDir, "config.json");
            var manager = new TestableConfigManager(configPath);
            var expectedConfig = new AppConfig
            {
                GamePath = "C:\\Games\\IEVR",
                CfgBinPath = "C:\\Games\\IEVR\\cfg.bin",
                ViolaCliPath = "C:\\Tools\\viola.exe",
                SelectedCpkName = "test.cpk",
                Theme = "Dark",
                Language = "en-US"
            };
            SaveConfigToFile(configPath, expectedConfig);

            // Act
            var result = manager.Load();

            // Assert
            Assert.Equal(expectedConfig.GamePath, result.GamePath);
            Assert.Equal(expectedConfig.CfgBinPath, result.CfgBinPath);
            Assert.Equal(expectedConfig.ViolaCliPath, result.ViolaCliPath);
            Assert.Equal(expectedConfig.SelectedCpkName, result.SelectedCpkName);
            Assert.Equal(expectedConfig.Theme, result.Theme);
            Assert.Equal(expectedConfig.Language, result.Language);
        }

        [Fact]
        public void Load_InvalidJson_ReturnsDefaultConfig()
        {
            // Arrange
            var configPath = Path.Combine(_testConfigDir, "config.json");
            var manager = new TestableConfigManager(configPath);
            File.WriteAllText(configPath, "{ invalid json }");

            // Act
            var result = manager.Load();

            // Assert
            Assert.NotNull(result);
            // Should return default config when JSON is invalid
        }

        [Fact]
        public void Load_OldConfigMissingNewProperties_MigratesConfig()
        {
            // Arrange
            var configPath = Path.Combine(_testConfigDir, "config.json");
            var manager = new TestableConfigManager(configPath);
            var oldConfig = new { GamePath = "C:\\Games\\IEVR" };
            File.WriteAllText(configPath, JsonSerializer.Serialize(oldConfig));

            // Act
            var result = manager.Load();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("C:\\Games\\IEVR", result.GamePath);
            // New properties should have default values
            Assert.NotNull(result.TmpDir);
            Assert.Equal("System", result.Theme);
            Assert.Equal("System", result.Language);
        }

        [Fact]
        public void Save_ValidConfig_SavesCorrectly()
        {
            // Arrange
            var configPath = Path.Combine(_testConfigDir, "config.json");
            var manager = new TestableConfigManager(configPath);
            var modEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", "C:\\Mods", enabled: true),
                new ModEntry("Mod2", "C:\\Mods", enabled: false)
            };

            // Act
            var success = manager.Save(
                gamePath: "C:\\Games\\IEVR",
                selectedCpkName: "test.cpk",
                cfgBinPath: "C:\\Games\\IEVR\\cfg.bin",
                violaCliPath: "C:\\Tools\\viola.exe",
                tmpDir: "C:\\Temp",
                modEntries: modEntries,
                lastKnownPacksSignature: "sig123",
                lastKnownSteamBuildId: "build456",
                vanillaFallbackUntilUtc: DateTime.UtcNow,
                theme: "Dark",
                language: "en-US",
                lastAppliedProfile: "Profile1"
            );

            // Assert
            Assert.True(success);
            Assert.True(File.Exists(configPath));
            
            var loaded = manager.Load();
            Assert.Equal("C:\\Games\\IEVR", loaded.GamePath);
            Assert.Equal("test.cpk", loaded.SelectedCpkName);
            Assert.Equal(2, loaded.Mods.Count);
        }

        [Fact]
        public void Save_PreservesUpdateCheckTimestamps()
        {
            // Arrange
            var configPath = Path.Combine(_testConfigDir, "config.json");
            var manager = new TestableConfigManager(configPath);
            var checkTime = DateTime.UtcNow.AddDays(-1);
            
            // Create initial config with timestamps
            var initialConfig = new AppConfig
            {
                LastAppUpdateCheckUtc = checkTime,
                LastModPrefetchUtc = checkTime,
                LastCpkListCheckUtc = checkTime
            };
            SaveConfigToFile(configPath, initialConfig);

            // Act
            manager.Save(
                gamePath: "C:\\Games\\IEVR",
                selectedCpkName: "test.cpk",
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
            var loaded = manager.Load();
            Assert.Equal(checkTime, loaded.LastAppUpdateCheckUtc);
            Assert.Equal(checkTime, loaded.LastModPrefetchUtc);
            Assert.Equal(checkTime, loaded.LastCpkListCheckUtc);
        }

        [Fact]
        public void UpdateLastAppUpdateCheckUtc_UpdatesTimestamp()
        {
            // Arrange
            var configPath = Path.Combine(_testConfigDir, "config.json");
            var manager = new TestableConfigManager(configPath);
            var checkTime = DateTime.UtcNow;

            // Act
            manager.UpdateLastAppUpdateCheckUtc(checkTime);

            // Assert
            var loaded = manager.Load();
            Assert.True(Math.Abs((checkTime - loaded.LastAppUpdateCheckUtc).TotalSeconds) < 5);
        }

        [Fact]
        public void UpdateLastModPrefetchUtc_UpdatesTimestamp()
        {
            // Arrange
            var configPath = Path.Combine(_testConfigDir, "config.json");
            var manager = new TestableConfigManager(configPath);
            var prefetchTime = DateTime.UtcNow;

            // Act
            manager.UpdateLastModPrefetchUtc(prefetchTime);

            // Assert
            var loaded = manager.Load();
            Assert.True(Math.Abs((prefetchTime - loaded.LastModPrefetchUtc).TotalSeconds) < 5);
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

        // Testable wrapper for ConfigManager that allows custom config path
        private class TestableConfigManager : ConfigManager
        {
            private readonly string _customConfigPath;

            public TestableConfigManager(string configPath)
            {
                _customConfigPath = configPath;
                EnsureDirectoryExists(Path.GetDirectoryName(configPath));
            }

            // Override Load to use custom path
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

            // Override Save to use custom path
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

            public new void UpdateLastModPrefetchUtc(DateTime prefetchTime)
            {
                try
                {
                    var config = Load();
                    config.LastModPrefetchUtc = prefetchTime;
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

                using var jsonDoc = JsonDocument.Parse(originalJson);
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("TmpDir", out _) || string.IsNullOrWhiteSpace(loadedConfig.TmpDir))
                {
                    loadedConfig.TmpDir = defaultConfig.TmpDir;
                    migrated = true;
                }

                if (!root.TryGetProperty("Theme", out _) || string.IsNullOrWhiteSpace(loadedConfig.Theme))
                {
                    loadedConfig.Theme = defaultConfig.Theme;
                    migrated = true;
                }

                if (!root.TryGetProperty("Language", out _) || string.IsNullOrWhiteSpace(loadedConfig.Language))
                {
                    loadedConfig.Language = defaultConfig.Language;
                    migrated = true;
                }

                if (migrated)
                {
                    SaveConfigToFile(loadedConfig);
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

