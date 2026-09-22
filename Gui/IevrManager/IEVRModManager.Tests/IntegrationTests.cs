using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IEVRModManager.Managers;
using IEVRModManager.Models;
using Xunit;

namespace IEVRModManager.Tests
{
    /// <summary>
    /// Integration tests for complete workflows across multiple managers.
    /// </summary>
    public class IntegrationTests : IDisposable
    {
        private readonly string _testDir;

        public IntegrationTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), $"IntegrationTests_{Guid.NewGuid()}");
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
        public void ConfigManager_ProfileManager_CompleteWorkflow()
        {
            // Arrange
            var configPath = Path.Combine(_testDir, "config.json");
            var profilesDir = Path.Combine(_testDir, "Profiles");
            Directory.CreateDirectory(profilesDir);
            
            var configManager = new TestableConfigManager(configPath);
            var profileManager = new TestableProfileManager(profilesDir);

            // Create initial config
            var modEntries = new List<ModEntry>
            {
                new ModEntry("Mod1", Path.Combine(_testDir, "Mods"), enabled: true),
                new ModEntry("Mod2", Path.Combine(_testDir, "Mods"), enabled: false)
            };

            configManager.Save(
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
                language: "en-US"
            );

            // Act - Create profile from current config
            var config = configManager.Load();
            var profile = new ModProfile
            {
                Name = "TestProfile",
                Mods = config.Mods,
                SelectedCpkName = config.SelectedCpkName
            };
            profileManager.SaveProfile(profile);

            // Load profile back
            var loadedProfile = profileManager.LoadProfile("TestProfile");

            // Assert
            Assert.NotNull(loadedProfile);
            Assert.Equal("TestProfile", loadedProfile.Name);
            Assert.Equal(2, loadedProfile.Mods.Count);
            Assert.Equal("test.cpk", loadedProfile.SelectedCpkName);
        }

        [Fact]
        public void ModManager_ConfigManager_ScanAndSaveWorkflow()
        {
            // Arrange
            var modsDir = Path.Combine(_testDir, "Mods");
            Directory.CreateDirectory(modsDir);
            Directory.CreateDirectory(Path.Combine(modsDir, "Mod1"));
            Directory.CreateDirectory(Path.Combine(modsDir, "Mod2"));
            Directory.CreateDirectory(Path.Combine(modsDir, "Mod3"));

            var configPath = Path.Combine(_testDir, "config.json");
            var modManager = new ModManager(modsDir);
            var configManager = new TestableConfigManager(configPath);

            // Act - Scan mods
            var scannedMods = modManager.ScanMods(savedMods: null, existingEntries: null);

            // Save to config
            configManager.Save(
                gamePath: "C:\\Games\\IEVR",
                selectedCpkName: "test.cpk",
                cfgBinPath: "",
                violaCliPath: "",
                tmpDir: "",
                modEntries: scannedMods,
                lastKnownPacksSignature: "",
                lastKnownSteamBuildId: "",
                vanillaFallbackUntilUtc: DateTime.MinValue,
                theme: "System",
                language: "System"
            );

            // Load config and verify
            var config = configManager.Load();

            // Assert
            Assert.Equal(3, config.Mods.Count);
            Assert.Contains(config.Mods, m => m.Name == "Mod1");
            Assert.Contains(config.Mods, m => m.Name == "Mod2");
            Assert.Contains(config.Mods, m => m.Name == "Mod3");
        }

        [Fact]
        public void ProfileManager_LastInstallManager_CompleteModApplicationWorkflow()
        {
            // Arrange
            var profilesDir = Path.Combine(_testDir, "Profiles");
            var recordPath = Path.Combine(_testDir, "last_install.json");
            Directory.CreateDirectory(profilesDir);

            var profileManager = new TestableProfileManager(profilesDir);
            var lastInstallManager = new TestableLastInstallManager(recordPath);

            // Create and save profile
            var profile = new ModProfile
            {
                Name = "MyProfile",
                Mods = new List<ModData>
                {
                    new ModData { Name = "Mod1", Enabled = true },
                    new ModData { Name = "Mod2", Enabled = true }
                },
                SelectedCpkName = "test.cpk"
            };
            profileManager.SaveProfile(profile);

            // Act - Simulate mod application
            var enabledMods = profile.Mods.Where(m => m.Enabled).Select(m => m.Name).ToList();
            var installInfo = new LastInstallInfo
            {
                GamePath = "C:\\Games\\IEVR",
                Mods = enabledMods,
                Files = new List<string> { "file1.txt", "file2.txt" },
                AppliedAt = DateTime.UtcNow,
                SelectedCpkName = profile.SelectedCpkName,
                SelectedCpkInfo = "Test CPK Info"
            };
            lastInstallManager.Save(installInfo);

            // Load and verify
            var loadedInfo = lastInstallManager.Load();

            // Assert
            Assert.Equal("C:\\Games\\IEVR", loadedInfo.GamePath);
            Assert.Equal(2, loadedInfo.Mods.Count);
            Assert.Contains("Mod1", loadedInfo.Mods);
            Assert.Contains("Mod2", loadedInfo.Mods);
            Assert.Equal("test.cpk", loadedInfo.SelectedCpkName);
        }

        [Fact]
        public void ConfigManager_ModManager_ProfileManager_FullCycle()
        {
            // Arrange
            var modsDir = Path.Combine(_testDir, "Mods");
            var configPath = Path.Combine(_testDir, "config.json");
            var profilesDir = Path.Combine(_testDir, "Profiles");
            Directory.CreateDirectory(modsDir);
            Directory.CreateDirectory(profilesDir);
            Directory.CreateDirectory(Path.Combine(modsDir, "Mod1"));
            Directory.CreateDirectory(Path.Combine(modsDir, "Mod2"));

            var modManager = new ModManager(modsDir);
            var configManager = new TestableConfigManager(configPath);
            var profileManager = new TestableProfileManager(profilesDir);

            // Act - Full cycle
            // 1. Scan mods
            var scannedMods = modManager.ScanMods(savedMods: null, existingEntries: null);

            // 2. Save to config
            configManager.Save(
                gamePath: "C:\\Games\\IEVR",
                selectedCpkName: "test.cpk",
                cfgBinPath: "",
                violaCliPath: "",
                tmpDir: "",
                modEntries: scannedMods,
                lastKnownPacksSignature: "",
                lastKnownSteamBuildId: "",
                vanillaFallbackUntilUtc: DateTime.MinValue,
                theme: "Dark",
                language: "en-US"
            );

            // 3. Create profile from config
            var config = configManager.Load();
            var profile = new ModProfile
            {
                Name = "SavedProfile",
                Mods = config.Mods,
                SelectedCpkName = config.SelectedCpkName
            };
            profileManager.SaveProfile(profile);

            // 4. Modify mods in config
            scannedMods[0].Enabled = false;
            configManager.Save(
                gamePath: "C:\\Games\\IEVR",
                selectedCpkName: "test.cpk",
                cfgBinPath: "",
                violaCliPath: "",
                tmpDir: "",
                modEntries: scannedMods,
                lastKnownPacksSignature: "",
                lastKnownSteamBuildId: "",
                vanillaFallbackUntilUtc: DateTime.MinValue,
                theme: "Dark",
                language: "en-US"
            );

            // 5. Load profile back
            var loadedProfile = profileManager.LoadProfile("SavedProfile");

            // Assert
            Assert.NotNull(loadedProfile);
            Assert.Equal(2, loadedProfile.Mods.Count);
            // Profile should have original state (both enabled)
            Assert.True(loadedProfile.Mods[0].Enabled);
        }

        [Fact]
        public void ModManager_DetectConflicts_WithProfileManager_Integration()
        {
            // Arrange
            var modsDir = Path.Combine(_testDir, "Mods");
            Directory.CreateDirectory(modsDir);
            Directory.CreateDirectory(Path.Combine(modsDir, "Mod1"));
            Directory.CreateDirectory(Path.Combine(modsDir, "Mod2"));
            
            // Create conflicting files
            var mod1File = Path.Combine(modsDir, "Mod1", "data", "file.txt");
            var mod2File = Path.Combine(modsDir, "Mod2", "data", "file.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(mod1File)!);
            Directory.CreateDirectory(Path.GetDirectoryName(mod2File)!);
            File.WriteAllText(mod1File, "Mod1 content");
            File.WriteAllText(mod2File, "Mod2 content");

            var modManager = new ModManager(modsDir);
            var profilesDir = Path.Combine(_testDir, "Profiles");
            Directory.CreateDirectory(profilesDir);
            var profileManager = new TestableProfileManager(profilesDir);

            // Act
            var scannedMods = modManager.ScanMods(savedMods: null, existingEntries: null);
            var conflicts = modManager.DetectFileConflicts(scannedMods);

            // Create profile with conflicting mods
            var profile = new ModProfile
            {
                Name = "ConflictingProfile",
                Mods = scannedMods.Select(m => new ModData { Name = m.Name, Enabled = m.Enabled }).ToList(),
                SelectedCpkName = "test.cpk"
            };
            profileManager.SaveProfile(profile);

            // Assert
            Assert.NotNull(conflicts);
            var loadedProfile = profileManager.LoadProfile("ConflictingProfile");
            Assert.NotNull(loadedProfile);
        }

        [Fact]
        public void ConfigManager_UpdateTimestamps_PreservesAcrossSaves()
        {
            // Arrange
            var configPath = Path.Combine(_testDir, "config.json");
            var manager = new TestableConfigManager(configPath);
            var checkTime1 = DateTime.UtcNow.AddDays(-2);
            var checkTime2 = DateTime.UtcNow.AddDays(-1);
            var prefetchTime = DateTime.UtcNow.AddHours(-12);

            // Act
            manager.UpdateLastAppUpdateCheckUtc(checkTime1);
            manager.UpdateLastModPrefetchUtc(prefetchTime);
            manager.UpdateLastAppUpdateCheckUtc(checkTime2);

            // Save config (should preserve timestamps)
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

            var config = manager.Load();

            // Assert
            Assert.Equal(checkTime2, config.LastAppUpdateCheckUtc);
            Assert.Equal(prefetchTime, config.LastModPrefetchUtc);
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

                    return config ?? AppConfig.Default();
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
                if (modEntries == null)
                {
                    throw new ArgumentNullException(nameof(modEntries));
                }

                try
                {
                    Helpers.FileSystemHelper.EnsureDirectoryExists(Path.GetDirectoryName(_customConfigPath));
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

            private void SaveConfigToFile(AppConfig config)
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                var json = System.Text.Json.JsonSerializer.Serialize(config, options);
                File.WriteAllText(_customConfigPath, json);
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

                var filePath = GetProfilePath(profileName);
                if (!File.Exists(filePath))
                {
                    return null;
                }

                try
                {
                    var json = File.ReadAllText(filePath);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return null;
                    }

                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    return System.Text.Json.JsonSerializer.Deserialize<ModProfile>(json, options);
                }
                catch
                {
                    return null;
                }
            }

            public new bool SaveProfile(ModProfile profile)
            {
                if (profile == null)
                {
                    throw new ArgumentNullException(nameof(profile));
                }

                try
                {
                    Helpers.FileSystemHelper.EnsureDirectoryExists(_customProfilesDir);
                    var safeName = GetSafeProfileFileName(profile.Name);
                    var filePath = Path.Combine(_customProfilesDir, $"{safeName}.json");

                    profile.LastModifiedDate = DateTime.Now;
                    if (profile.CreatedDate == DateTime.MinValue)
                    {
                        profile.CreatedDate = DateTime.Now;
                    }

                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    var json = System.Text.Json.JsonSerializer.Serialize(profile, options);
                    File.WriteAllText(filePath, json);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            private string GetProfilePath(string profileName)
            {
                var safeName = SanitizeFileName(profileName);
                return Path.Combine(_customProfilesDir, $"{safeName}.json");
            }

            private string GetSafeProfileFileName(string profileName)
            {
                var safeName = SanitizeFileName(profileName);
                return string.IsNullOrWhiteSpace(safeName) ? "Unnamed" : safeName;
            }

            private static string SanitizeFileName(string fileName)
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return "Unnamed";
                }

                var invalidChars = Path.GetInvalidFileNameChars();
                var sanitized = new string(fileName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());

                if (sanitized.Length > 100)
                {
                    sanitized = sanitized.Substring(0, 100);
                }

                return sanitized.Trim();
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

                try
                {
                    Helpers.FileSystemHelper.EnsureDirectoryExists(Path.GetDirectoryName(_customRecordPath));
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(info, options);
                    File.WriteAllText(_customRecordPath, json);
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
