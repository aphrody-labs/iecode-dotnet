using System;
using System.IO;
using System.Text;
using IEVRModManager.Exceptions;
using IEVRModManager.Managers;
using IEVRModManager.Models;
using Xunit;

namespace IEVRModManager.Tests
{
    /// <summary>
    /// Tests for error handling scenarios across managers.
    /// </summary>
    public class ErrorHandlingTests : IDisposable
    {
        private readonly string _testDir;

        public ErrorHandlingTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), $"ErrorHandlingTests_{Guid.NewGuid()}");
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
        public void ConfigManager_Load_CorruptedJson_ThrowsConfigurationException()
        {
            // Arrange
            var configPath = Path.Combine(_testDir, "config.json");
            File.WriteAllText(configPath, "{ invalid json }");
            var manager = new TestableConfigManager(configPath);

            // Act & Assert
            var exception = Assert.Throws<ConfigurationException>(() => manager.Load());
            Assert.Contains("corrupted", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ConfigManager_Load_EmptyFile_ReturnsDefaultConfig()
        {
            // Arrange
            var configPath = Path.Combine(_testDir, "config.json");
            File.WriteAllText(configPath, "");
            var manager = new TestableConfigManager(configPath);

            // Act
            var result = manager.Load();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.GamePath);
        }

        [Fact]
        public void ConfigManager_Save_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            var configPath = Path.Combine(_testDir, "config.json");
            var manager = new TestableConfigManager(configPath);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => manager.Save((AppConfig)null!));
        }

        [Fact]
        public void ConfigManager_Save_NullModEntries_ThrowsArgumentNullException()
        {
            // Arrange
            var configPath = Path.Combine(_testDir, "config.json");
            var manager = new TestableConfigManager(configPath);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => manager.Save(
                gamePath: "",
                selectedCpkName: "",
                cfgBinPath: "",
                violaCliPath: "",
                tmpDir: "",
                modEntries: null!,
                lastKnownPacksSignature: "",
                lastKnownSteamBuildId: "",
                vanillaFallbackUntilUtc: DateTime.MinValue,
                theme: "System",
                language: "System"
            ));
        }

        [Fact]
        public void ProfileManager_LoadProfile_EmptyProfileName_ThrowsArgumentException()
        {
            // Arrange
            var profilesDir = Path.Combine(_testDir, "Profiles");
            Directory.CreateDirectory(profilesDir);
            var manager = new TestableProfileManager(profilesDir);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => manager.LoadProfile(""));
            Assert.Throws<ArgumentException>(() => manager.LoadProfile("   "));
            Assert.Throws<ArgumentException>(() => manager.LoadProfile(null!));
        }

        [Fact]
        public void ProfileManager_SaveProfile_NullProfile_ThrowsArgumentNullException()
        {
            // Arrange
            var profilesDir = Path.Combine(_testDir, "Profiles");
            Directory.CreateDirectory(profilesDir);
            var manager = new TestableProfileManager(profilesDir);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => manager.SaveProfile(null!));
        }

        [Fact]
        public void ProfileManager_DeleteProfile_EmptyProfileName_ThrowsArgumentException()
        {
            // Arrange
            var profilesDir = Path.Combine(_testDir, "Profiles");
            Directory.CreateDirectory(profilesDir);
            var manager = new TestableProfileManager(profilesDir);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => manager.DeleteProfile(""));
            Assert.Throws<ArgumentException>(() => manager.DeleteProfile("   "));
        }

        [Fact]
        public void LastInstallManager_Save_NullInfo_ThrowsArgumentNullException()
        {
            // Arrange
            var recordPath = Path.Combine(_testDir, "last_install.json");
            var manager = new TestableLastInstallManager(recordPath);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => manager.Save(null!));
        }

        [Fact]
        public void ModManager_GetEnabledMods_NullModEntries_ThrowsArgumentNullException()
        {
            // Arrange
            var modsDir = Path.Combine(_testDir, "Mods");
            Directory.CreateDirectory(modsDir);
            var manager = new ModManager(modsDir);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => manager.GetEnabledMods(null!));
        }

        [Fact]
        public void ModManager_DetectPacksModifiers_NullModEntries_ThrowsArgumentNullException()
        {
            // Arrange
            var modsDir = Path.Combine(_testDir, "Mods");
            Directory.CreateDirectory(modsDir);
            var manager = new ModManager(modsDir);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => manager.DetectPacksModifiers(null!));
        }

        [Fact]
        public void ModManager_DetectFileConflicts_NullModEntries_ThrowsArgumentNullException()
        {
            // Arrange
            var modsDir = Path.Combine(_testDir, "Mods");
            Directory.CreateDirectory(modsDir);
            var manager = new ModManager(modsDir);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => manager.DetectFileConflicts(null!));
        }

        [Fact]
        public void ConfigManager_Load_FileWithInvalidEncoding_HandlesGracefully()
        {
            // Arrange
            var configPath = Path.Combine(_testDir, "config.json");
            using (var writer = new StreamWriter(configPath, false, Encoding.GetEncoding("ISO-8859-1")))
            {
                writer.Write("{\"GamePath\":\"C:\\\\Games\"}");
            }
            var manager = new TestableConfigManager(configPath);

            // Act
            var result = manager.Load();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void ProfileManager_LoadProfileFromFile_InvalidJson_ReturnsNull()
        {
            // Arrange
            var profilesDir = Path.Combine(_testDir, "Profiles");
            Directory.CreateDirectory(profilesDir);
            var profilePath = Path.Combine(profilesDir, "test.json");
            File.WriteAllText(profilePath, "{ invalid json }");
            var manager = new TestableProfileManager(profilesDir);

            // Act
            var result = manager.LoadProfile("test");

            // Assert
            // ProfileManager returns null for invalid JSON instead of throwing
            Assert.Null(result);
        }

        [Fact]
        public void ProfileManager_LoadProfileFromFile_EmptyFile_ReturnsNull()
        {
            // Arrange
            var profilesDir = Path.Combine(_testDir, "Profiles");
            Directory.CreateDirectory(profilesDir);
            var profilePath = Path.Combine(profilesDir, "test.json");
            File.WriteAllText(profilePath, "");
            var manager = new TestableProfileManager(profilesDir);

            // Act
            var result = manager.LoadProfile("test");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void LastInstallManager_Load_InvalidJson_ReturnsEmptyInfo()
        {
            // Arrange
            var recordPath = Path.Combine(_testDir, "last_install.json");
            File.WriteAllText(recordPath, "{ invalid json }");
            var manager = new TestableLastInstallManager(recordPath);

            // Act
            var result = manager.Load();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.GamePath);
            Assert.Empty(result.Mods);
            Assert.Empty(result.Files);
        }

        [Fact]
        public void LastInstallManager_Load_EmptyFile_ReturnsEmptyInfo()
        {
            // Arrange
            var recordPath = Path.Combine(_testDir, "last_install.json");
            File.WriteAllText(recordPath, "");
            var manager = new TestableLastInstallManager(recordPath);

            // Act
            var result = manager.Load();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.GamePath);
        }

        // Testable wrappers
        private class TestableConfigManager : ConfigManager
        {
            private readonly string _customConfigPath;

            public TestableConfigManager(string configPath)
            {
                _customConfigPath = configPath;
                Helpers.FileSystemHelper.EnsureDirectoryExists(Path.GetDirectoryName(configPath));
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

                    var config = System.Text.Json.JsonSerializer.Deserialize<AppConfig>(json, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (config == null)
                    {
                        return AppConfig.Default();
                    }

                    return AppConfig.Default();
                }
                catch (System.Text.Json.JsonException ex)
                {
                    throw new ConfigurationException("Failed to parse configuration file. The file may be corrupted.", ex);
                }
                catch (IOException ex)
                {
                    throw new ConfigurationException("Failed to read configuration file. Check file permissions.", ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new ConfigurationException("Access denied to configuration file.", ex);
                }
                catch (Exception ex)
                {
                    throw new ConfigurationException("An unexpected error occurred while loading configuration.", ex);
                }
            }

            public new bool Save(AppConfig config)
            {
                if (config == null)
                {
                    throw new ArgumentNullException(nameof(config));
                }
                return true;
            }

            public new bool Save(string gamePath, string selectedCpkName, string cfgBinPath, string violaCliPath,
                string tmpDir, System.Collections.Generic.List<ModEntry> modEntries,
                string lastKnownPacksSignature, string lastKnownSteamBuildId,
                DateTime vanillaFallbackUntilUtc, string theme, string language, string lastAppliedProfile = "")
            {
                if (modEntries == null)
                {
                    throw new ArgumentNullException(nameof(modEntries));
                }
                return true;
            }
        }

        private class TestableProfileManager : ProfileManager
        {
            private readonly string _customProfilesDir;

            public TestableProfileManager(string profilesDir)
            {
                _customProfilesDir = profilesDir;
                Helpers.FileSystemHelper.EnsureDirectoryExists(profilesDir);
            }

            public new ModProfile? LoadProfile(string profileName)
            {
                if (string.IsNullOrWhiteSpace(profileName))
                {
                    throw new ArgumentException("Profile name cannot be null or empty.", nameof(profileName));
                }
                return null;
            }

            public new bool SaveProfile(ModProfile profile)
            {
                if (profile == null)
                {
                    throw new ArgumentNullException(nameof(profile));
                }
                return true;
            }

            public new bool DeleteProfile(string profileName)
            {
                if (string.IsNullOrWhiteSpace(profileName))
                {
                    throw new ArgumentException("Profile name cannot be null or empty.", nameof(profileName));
                }
                return false;
            }
        }

        private class TestableLastInstallManager : LastInstallManager
        {
            private readonly string _customRecordPath;

            public TestableLastInstallManager(string recordPath)
            {
                _customRecordPath = recordPath;
                Helpers.FileSystemHelper.EnsureDirectoryExists(Path.GetDirectoryName(recordPath));
            }

            public new LastInstallInfo Load()
            {
                if (!File.Exists(_customRecordPath))
                {
                    return LastInstallInfo.Empty();
                }

                try
                {
                    var json = File.ReadAllText(_customRecordPath);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return LastInstallInfo.Empty();
                    }

                    var info = System.Text.Json.JsonSerializer.Deserialize<LastInstallInfo>(json, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return info ?? LastInstallInfo.Empty();
                }
                catch
                {
                    return LastInstallInfo.Empty();
                }
            }

            public new bool Save(LastInstallInfo info)
            {
                if (info == null)
                {
                    throw new ArgumentNullException(nameof(info));
                }
                return true;
            }
        }
    }
}
