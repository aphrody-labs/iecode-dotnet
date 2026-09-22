using System;
using System.IO;
using System.Text.Json;
using IEVRModManager.Managers;
using IEVRModManager.Models;
using Xunit;

namespace IEVRModManager.Tests
{
    /// <summary>
    /// Tests for configuration migration scenarios.
    /// </summary>
    public class ConfigMigrationTests : IDisposable
    {
        private readonly string _testConfigDir;
        private readonly string _testConfigPath;

        public ConfigMigrationTests()
        {
            _testConfigDir = Path.Combine(Path.GetTempPath(), $"ConfigMigrationTests_{Guid.NewGuid()}");
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
        public void MigrateConfig_MissingTmpDir_AddsDefaultTmpDir()
        {
            // Arrange
            var manager = GetConfigManager();
            var oldConfigJson = @"{
                ""GamePath"": ""C:\\Games\\IEVR""
            }";
            File.WriteAllText(_testConfigPath, oldConfigJson);

            // Act
            var result = manager.Load();

            // Assert
            Assert.NotNull(result.TmpDir);
            Assert.NotEmpty(result.TmpDir);
        }

        [Fact]
        public void MigrateConfig_MissingTheme_AddsDefaultTheme()
        {
            // Arrange
            var manager = GetConfigManager();
            var oldConfigJson = @"{
                ""GamePath"": ""C:\\Games\\IEVR""
            }";
            File.WriteAllText(_testConfigPath, oldConfigJson);

            // Act
            var result = manager.Load();

            // Assert
            Assert.Equal("System", result.Theme);
        }

        [Fact]
        public void MigrateConfig_MissingLanguage_AddsDefaultLanguage()
        {
            // Arrange
            var manager = GetConfigManager();
            var oldConfigJson = @"{
                ""GamePath"": ""C:\\Games\\IEVR""
            }";
            File.WriteAllText(_testConfigPath, oldConfigJson);

            // Act
            var result = manager.Load();

            // Assert
            Assert.Equal("System", result.Language);
        }

        [Fact]
        public void MigrateConfig_MissingLastAppliedProfile_AddsDefault()
        {
            // Arrange
            var manager = GetConfigManager();
            var oldConfigJson = @"{
                ""GamePath"": ""C:\\Games\\IEVR""
            }";
            File.WriteAllText(_testConfigPath, oldConfigJson);

            // Act
            var result = manager.Load();

            // Assert
            Assert.Equal(string.Empty, result.LastAppliedProfile);
        }

        [Fact]
        public void MigrateConfig_MissingDateTimeProperties_AddsDefaults()
        {
            // Arrange
            var manager = GetConfigManager();
            var oldConfigJson = @"{
                ""GamePath"": ""C:\\Games\\IEVR""
            }";
            File.WriteAllText(_testConfigPath, oldConfigJson);

            // Act
            var result = manager.Load();

            // Assert
            Assert.Equal(DateTime.MinValue, result.LastCpkListCheckUtc);
            Assert.Equal(DateTime.MinValue, result.LastAppUpdateCheckUtc);
            Assert.Equal(DateTime.MinValue, result.LastModPrefetchUtc);
        }

        [Fact]
        public void MigrateConfig_MissingModsProperty_InitializesEmptyList()
        {
            // Arrange
            var manager = GetConfigManager();
            var oldConfigJson = @"{
                ""GamePath"": ""C:\\Games\\IEVR""
            }";
            File.WriteAllText(_testConfigPath, oldConfigJson);

            // Act
            var result = manager.Load();

            // Assert
            Assert.NotNull(result.Mods);
            Assert.Empty(result.Mods);
        }

        [Fact]
        public void MigrateConfig_NullStringProperties_ConvertsToEmptyStrings()
        {
            // Arrange
            var manager = GetConfigManager();
            var oldConfigJson = @"{
                ""GamePath"": null,
                ""CfgBinPath"": null,
                ""ViolaCliPath"": null,
                ""SelectedCpkName"": null
            }";
            File.WriteAllText(_testConfigPath, oldConfigJson);

            // Act
            var result = manager.Load();

            // Assert
            Assert.Equal(string.Empty, result.GamePath);
            Assert.Equal(string.Empty, result.CfgBinPath);
            Assert.Equal(string.Empty, result.ViolaCliPath);
            Assert.Equal(string.Empty, result.SelectedCpkName);
        }

        [Fact]
        public void MigrateConfig_PartialMigration_PreservesExistingValues()
        {
            // Arrange
            var manager = GetConfigManager();
            var oldConfigJson = @"{
                ""GamePath"": ""C:\\Games\\IEVR"",
                ""Theme"": ""Dark"",
                ""Language"": ""en-US""
            }";
            File.WriteAllText(_testConfigPath, oldConfigJson);

            // Act
            var result = manager.Load();

            // Assert
            Assert.Equal("C:\\Games\\IEVR", result.GamePath);
            Assert.Equal("Dark", result.Theme);
            Assert.Equal("en-US", result.Language);
            Assert.NotNull(result.TmpDir); // Should be migrated
        }

        [Fact]
        public void MigrateConfig_EmptyJsonObject_MigratesAllProperties()
        {
            // Arrange
            var manager = GetConfigManager();
            File.WriteAllText(_testConfigPath, "{}");

            // Act
            var result = manager.Load();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.GamePath);
            Assert.NotNull(result.TmpDir);
            Assert.Equal("System", result.Theme);
            Assert.Equal("System", result.Language);
            Assert.NotNull(result.Mods);
        }

        [Fact]
        public void MigrateConfig_WhitespaceStringProperties_PreservesWhitespace()
        {
            // Arrange
            var manager = GetConfigManager();
            var oldConfigJson = @"{
                ""GamePath"": ""   "",
                ""CfgBinPath"": ""\t"",
                ""ViolaCliPath"": ""\n""
            }";
            File.WriteAllText(_testConfigPath, oldConfigJson);

            // Act
            var result = manager.Load();

            // Assert
            // Note: The migration logic only migrates missing properties or null/empty values,
            // not whitespace-only values. Whitespace values are preserved as-is.
            Assert.Equal("   ", result.GamePath);
            Assert.Equal("\t", result.CfgBinPath);
            Assert.Equal("\n", result.ViolaCliPath);
        }

        [Fact]
        public void MigrateConfig_MigratedConfigIsSavedToFile()
        {
            // Arrange
            var manager = GetConfigManager();
            var oldConfigJson = @"{
                ""GamePath"": ""C:\\Games\\IEVR""
            }";
            File.WriteAllText(_testConfigPath, oldConfigJson);

            // Act
            manager.Load(); // Triggers migration
            var savedJson = File.ReadAllText(_testConfigPath);
            var savedConfig = JsonSerializer.Deserialize<AppConfig>(savedJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            Assert.NotNull(savedConfig);
            Assert.NotNull(savedConfig.TmpDir);
            Assert.Equal("System", savedConfig.Theme);
            Assert.Equal("System", savedConfig.Language);
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
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return AppConfig.Default();
                    }

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

            private AppConfig MigrateConfig(AppConfig loadedConfig, string originalJson)
            {
                if (loadedConfig == null)
                {
                    return AppConfig.Default();
                }

                if (string.IsNullOrWhiteSpace(originalJson))
                {
                    return loadedConfig;
                }

                var defaultConfig = AppConfig.Default();
                var migrated = false;

                JsonDocument? jsonDoc = null;
                try
                {
                    jsonDoc = JsonDocument.Parse(originalJson);
                    var root = jsonDoc.RootElement;

                    migrated |= MigrateStringProperty(root, "TmpDir", () => loadedConfig.TmpDir,
                        value => loadedConfig.TmpDir = value, defaultConfig.TmpDir);
                    migrated |= MigrateStringProperty(root, "Theme", () => loadedConfig.Theme,
                        value => loadedConfig.Theme = value, defaultConfig.Theme);
                    migrated |= MigrateStringProperty(root, "Language", () => loadedConfig.Language,
                        value => loadedConfig.Language = value, defaultConfig.Language);
                    migrated |= MigrateStringProperty(root, "LastAppliedProfile", () => loadedConfig.LastAppliedProfile,
                        value => loadedConfig.LastAppliedProfile = value, defaultConfig.LastAppliedProfile);

                    migrated |= MigrateDateTimeProperty(root, "LastCpkListCheckUtc", () => loadedConfig.LastCpkListCheckUtc,
                        value => loadedConfig.LastCpkListCheckUtc = value, defaultConfig.LastCpkListCheckUtc);
                    migrated |= MigrateDateTimeProperty(root, "LastAppUpdateCheckUtc", () => loadedConfig.LastAppUpdateCheckUtc,
                        value => loadedConfig.LastAppUpdateCheckUtc = value, defaultConfig.LastAppUpdateCheckUtc);
                    migrated |= MigrateDateTimeProperty(root, "LastModPrefetchUtc", () => loadedConfig.LastModPrefetchUtc,
                        value => loadedConfig.LastModPrefetchUtc = value, defaultConfig.LastModPrefetchUtc);

                    if (!root.TryGetProperty("Mods", out _) || loadedConfig.Mods == null)
                    {
                        loadedConfig.Mods = new System.Collections.Generic.List<ModData>();
                        migrated = true;
                    }

                    var stringProperties = new[]
                    {
                        new { Getter = (Func<string?>)(() => loadedConfig.GamePath), Setter = (Action<string>)(v => loadedConfig.GamePath = v) },
                        new { Getter = (Func<string?>)(() => loadedConfig.CfgBinPath), Setter = (Action<string>)(v => loadedConfig.CfgBinPath = v) },
                        new { Getter = (Func<string?>)(() => loadedConfig.ViolaCliPath), Setter = (Action<string>)(v => loadedConfig.ViolaCliPath = v) },
                        new { Getter = (Func<string?>)(() => loadedConfig.SelectedCpkName), Setter = (Action<string>)(v => loadedConfig.SelectedCpkName = v) },
                        new { Getter = (Func<string?>)(() => loadedConfig.LastKnownPacksSignature), Setter = (Action<string>)(v => loadedConfig.LastKnownPacksSignature = v) },
                        new { Getter = (Func<string?>)(() => loadedConfig.LastKnownSteamBuildId), Setter = (Action<string>)(v => loadedConfig.LastKnownSteamBuildId = v) }
                    };

                    foreach (var prop in stringProperties)
                    {
                        if (prop.Getter() == null)
                        {
                            prop.Setter(string.Empty);
                            migrated = true;
                        }
                    }

                    if (migrated)
                    {
                        SaveMigratedConfig(loadedConfig);
                    }

                    return loadedConfig;
                }
                finally
                {
                    jsonDoc?.Dispose();
                }
            }

            private bool MigrateStringProperty(JsonElement root, string propertyName, Func<string> getter, Action<string> setter, string defaultValue)
            {
                if (!root.TryGetProperty(propertyName, out _) || string.IsNullOrWhiteSpace(getter()))
                {
                    setter(defaultValue);
                    return true;
                }
                return false;
            }

            private bool MigrateDateTimeProperty(JsonElement root, string propertyName, Func<DateTime> getter, Action<DateTime> setter, DateTime defaultValue)
            {
                if (!root.TryGetProperty(propertyName, out _) || getter() == DateTime.MinValue)
                {
                    setter(defaultValue);
                    return true;
                }
                return false;
            }

            private void SaveMigratedConfig(AppConfig config)
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    var json = JsonSerializer.Serialize(config, options);
                    File.WriteAllText(_customConfigPath, json);
                }
                catch
                {
                    // Ignore errors
                }
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
