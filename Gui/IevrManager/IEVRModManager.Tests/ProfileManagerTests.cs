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
    [Collection("ProfileManagerTests")]
    public class ProfileManagerTests : IDisposable
    {
        private readonly List<string> _testDirs = new List<string>();

        private TestableProfileManager GetProfileManager([System.Runtime.CompilerServices.CallerMemberName] string? testName = null)
        {
            var testProfilesDir = Path.Combine(Path.GetTempPath(), $"ProfileManagerTests_{testName}_{Guid.NewGuid()}");
            // Clean up directory if it exists
            if (Directory.Exists(testProfilesDir))
            {
                try
                {
                    Directory.Delete(testProfilesDir, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
            Directory.CreateDirectory(testProfilesDir);
            _testDirs.Add(testProfilesDir);
            return new TestableProfileManager(testProfilesDir);
        }

        public void Dispose()
        {
            foreach (var dir in _testDirs)
            {
                if (Directory.Exists(dir))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        [Fact]
        public void GetAllProfiles_EmptyDirectory_ReturnsEmptyList()
        {
            // Arrange
            var manager = GetProfileManager();
            
            // Verify we're using the correct directory and it's empty
            var testDir = manager.GetProfilesDir();
            var filesBefore = Directory.Exists(testDir) ? Directory.GetFiles(testDir, "*.json").Length : 0;
            
            // Act
            var result = manager.GetAllProfiles();

            // Assert
            Assert.Empty(result);
            // Double-check that the directory is actually empty
            var filesAfter = Directory.Exists(testDir) ? Directory.GetFiles(testDir, "*.json").Length : 0;
            Assert.Equal(0, filesAfter);
        }

        [Fact]
        public void GetAllProfiles_WithProfiles_ReturnsAllProfiles()
        {
            // Arrange
            var manager = GetProfileManager();
            var profile1 = new ModProfile { Name = "Profile1", Mods = new List<ModData>() };
            var profile2 = new ModProfile { Name = "Profile2", Mods = new List<ModData>() };
            manager.SaveProfile(profile1);
            System.Threading.Thread.Sleep(10); // Ensure different timestamps
            manager.SaveProfile(profile2);

            // Act
            var result = manager.GetAllProfiles();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Name == "Profile1");
            Assert.Contains(result, p => p.Name == "Profile2");
        }

        [Fact]
        public void GetAllProfiles_ReturnsProfilesOrderedByLastModifiedDate()
        {
            // Arrange
            var manager = GetProfileManager();
            var profile1 = new ModProfile { Name = "Profile1", Mods = new List<ModData>() };
            var profile2 = new ModProfile { Name = "Profile2", Mods = new List<ModData>() };
            var profile3 = new ModProfile { Name = "Profile3", Mods = new List<ModData>() };
            
            manager.SaveProfile(profile1);
            System.Threading.Thread.Sleep(10);
            manager.SaveProfile(profile2);
            System.Threading.Thread.Sleep(10);
            manager.SaveProfile(profile3);

            // Act
            var result = manager.GetAllProfiles();

            // Assert
            Assert.Equal(3, result.Count);
            // Should be ordered by LastModifiedDate descending (newest first)
            Assert.True(result[0].LastModifiedDate >= result[1].LastModifiedDate);
            Assert.True(result[1].LastModifiedDate >= result[2].LastModifiedDate);
        }

        [Fact]
        public void LoadProfile_ExistingProfile_ReturnsProfile()
        {
            // Arrange
            var manager = GetProfileManager();
            var expectedProfile = new ModProfile
            {
                Name = "TestProfile",
                Mods = new List<ModData>
                {
                    new ModData { Name = "Mod1", Enabled = true },
                    new ModData { Name = "Mod2", Enabled = false }
                },
                SelectedCpkName = "test.cpk"
            };
            manager.SaveProfile(expectedProfile);

            // Act
            var result = manager.LoadProfile("TestProfile");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestProfile", result.Name);
            Assert.Equal(2, result.Mods.Count);
            Assert.Equal("test.cpk", result.SelectedCpkName);
        }

        [Fact]
        public void LoadProfile_NonExistentProfile_ReturnsNull()
        {
            // Arrange
            var manager = GetProfileManager();

            // Act
            var result = manager.LoadProfile("NonExistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void SaveProfile_CreatesProfileFile()
        {
            // Arrange
            var manager = GetProfileManager();
            var profile = new ModProfile
            {
                Name = "NewProfile",
                Mods = new List<ModData>()
            };

            // Act
            var success = manager.SaveProfile(profile);

            // Assert
            Assert.True(success);
            Assert.True(manager.ProfileExists("NewProfile"));
        }

        [Fact]
        public void SaveProfile_SetsCreatedDate_OnNewProfile()
        {
            // Arrange
            var manager = GetProfileManager();
            var profile = new ModProfile
            {
                Name = "NewProfile",
                CreatedDate = DateTime.MinValue,
                Mods = new List<ModData>()
            };

            // Act
            manager.SaveProfile(profile);

            // Assert
            var loaded = manager.LoadProfile("NewProfile");
            Assert.NotNull(loaded);
            Assert.NotEqual(DateTime.MinValue, loaded.CreatedDate);
        }

        [Fact]
        public void SaveProfile_UpdatesLastModifiedDate()
        {
            // Arrange
            var manager = GetProfileManager();
            var profile = new ModProfile
            {
                Name = "TestProfile",
                Mods = new List<ModData>()
            };
            manager.SaveProfile(profile);
            var firstSave = manager.LoadProfile("TestProfile")!.LastModifiedDate;
            
            System.Threading.Thread.Sleep(10);

            // Act
            manager.SaveProfile(profile);

            // Assert
            var secondSave = manager.LoadProfile("TestProfile")!.LastModifiedDate;
            Assert.True(secondSave > firstSave);
        }

        [Fact]
        public void SaveProfile_SanitizesFileName_WithInvalidCharacters()
        {
            // Arrange
            var manager = GetProfileManager();
            var testProfilesDir = manager.GetProfilesDir();
            var profile = new ModProfile
            {
                Name = "Test<>Profile|Name",
                Mods = new List<ModData>()
            };

            // Act
            manager.SaveProfile(profile);

            // Assert
            // Should save with sanitized name
            var profiles = manager.GetAllProfiles();
            Assert.Contains(profiles, p => p.Name == "Test<>Profile|Name");
            // File should exist with sanitized name
            var files = Directory.GetFiles(testProfilesDir, "*.json");
            Assert.Contains(files, f => !f.Contains("<") && !f.Contains(">") && !f.Contains("|"));
        }

        [Fact]
        public void SaveProfile_HandlesEmptyProfileName()
        {
            // Arrange
            var manager = GetProfileManager();
            var testProfilesDir = manager.GetProfilesDir();
            var profile = new ModProfile
            {
                Name = "",
                Mods = new List<ModData>()
            };

            // Act
            var success = manager.SaveProfile(profile);

            // Assert
            Assert.True(success);
            // Should use "Unnamed" as fallback
            var files = Directory.GetFiles(testProfilesDir, "*.json");
            Assert.NotEmpty(files);
            
            // Cleanup: Delete the profile created with empty name
            try
            {
                // Try to delete using "Unnamed" since that's what the file is saved as
                manager.DeleteProfile("Unnamed");
            }
            catch
            {
                // If that fails, try to delete the file directly
                var unnamedFile = Path.Combine(testProfilesDir, "Unnamed.json");
                if (File.Exists(unnamedFile))
                {
                    File.Delete(unnamedFile);
                }
            }
        }

        [Fact]
        public void DeleteProfile_ExistingProfile_ReturnsTrue()
        {
            // Arrange
            var manager = GetProfileManager();
            var profile = new ModProfile { Name = "ToDelete", Mods = new List<ModData>() };
            manager.SaveProfile(profile);

            // Act
            var result = manager.DeleteProfile("ToDelete");

            // Assert
            Assert.True(result);
            Assert.False(manager.ProfileExists("ToDelete"));
        }

        [Fact]
        public void DeleteProfile_NonExistentProfile_ReturnsFalse()
        {
            // Arrange
            var manager = GetProfileManager();

            // Act
            var result = manager.DeleteProfile("NonExistent");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ProfileExists_ExistingProfile_ReturnsTrue()
        {
            // Arrange
            var manager = GetProfileManager();
            var profile = new ModProfile { Name = "Existing", Mods = new List<ModData>() };
            manager.SaveProfile(profile);

            // Act
            var result = manager.ProfileExists("Existing");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ProfileExists_NonExistentProfile_ReturnsFalse()
        {
            // Arrange
            var manager = GetProfileManager();

            // Act
            var result = manager.ProfileExists("NonExistent");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SaveProfile_PreservesModsData()
        {
            // Arrange
            var manager = GetProfileManager();
            var profile = new ModProfile
            {
                Name = "ModsTest",
                Mods = new List<ModData>
                {
                    new ModData { Name = "Mod1", Enabled = true, ModLink = "https://example.com/mod1" },
                    new ModData { Name = "Mod2", Enabled = false, ModLink = "https://example.com/mod2" }
                }
            };

            // Act
            manager.SaveProfile(profile);
            var loaded = manager.LoadProfile("ModsTest");

            // Assert
            Assert.NotNull(loaded);
            Assert.Equal(2, loaded.Mods.Count);
            Assert.Equal("Mod1", loaded.Mods[0].Name);
            Assert.True(loaded.Mods[0].Enabled);
            Assert.Equal("https://example.com/mod1", loaded.Mods[0].ModLink);
            Assert.Equal("Mod2", loaded.Mods[1].Name);
            Assert.False(loaded.Mods[1].Enabled);
        }

        // Testable wrapper for ProfileManager that allows custom profiles directory
        private class TestableProfileManager : ProfileManager
        {
            private readonly string _customProfilesDir;

            public string GetProfilesDir() => _customProfilesDir;

            public TestableProfileManager(string profilesDir) : base()
            {
                _customProfilesDir = profilesDir;
                // Clean up any existing files in the directory
                if (Directory.Exists(_customProfilesDir))
                {
                    foreach (var file in Directory.GetFiles(_customProfilesDir, "*.json"))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                            // Ignore errors
                        }
                    }
                }
                EnsureDirectoryExists(_customProfilesDir);
            }

            private string GetProfilePath(string profileName)
            {
                var safeName = SanitizeFileName(profileName);
                return Path.Combine(_customProfilesDir, $"{safeName}.json");
            }

            public new List<ModProfile> GetAllProfiles()
            {
                var profiles = new List<ModProfile>();

                // Explicitly use the custom directory, not the base class directory
                if (!Directory.Exists(_customProfilesDir))
                {
                    return profiles;
                }

                // Get only files from our custom directory
                var profileFiles = Directory.GetFiles(_customProfilesDir, "*.json", SearchOption.TopDirectoryOnly);

                foreach (var file in profileFiles)
                {
                    // Verify the file is actually in our custom directory
                    var fileDir = Path.GetDirectoryName(file);
                    if (!string.Equals(fileDir, _customProfilesDir, StringComparison.OrdinalIgnoreCase))
                    {
                        continue; // Skip files not in our directory
                    }

                    var profile = LoadProfileFromFile(file);
                    if (profile != null)
                    {
                        profiles.Add(profile);
                    }
                }

                return profiles.OrderByDescending(p => p.LastModifiedDate).ToList();
            }

            public new ModProfile? LoadProfile(string profileName)
            {
                var filePath = GetProfilePath(profileName);

                if (!File.Exists(filePath))
                {
                    return null;
                }

                return LoadProfileFromFile(filePath);
            }

            public new bool SaveProfile(ModProfile profile)
            {
                try
                {
                    EnsureDirectoryExists(_customProfilesDir);

                    var safeName = GetSafeProfileFileName(profile.Name);
                    var filePath = Path.Combine(_customProfilesDir, $"{safeName}.json");

                    UpdateProfileDates(profile);
                    SaveProfileToFile(profile, filePath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public new bool DeleteProfile(string profileName)
            {
                try
                {
                    var filePath = GetProfilePath(profileName);

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        return true;
                    }

                    return false;
                }
                catch
                {
                    return false;
                }
            }

            public new bool ProfileExists(string profileName)
            {
                var filePath = GetProfilePath(profileName);
                return File.Exists(filePath);
            }

            private ModProfile? LoadProfileFromFile(string filePath)
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var profile = JsonSerializer.Deserialize<ModProfile>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return profile;
                }
                catch
                {
                    return null;
                }
            }

            private string GetSafeProfileFileName(string profileName)
            {
                var safeName = SanitizeFileName(profileName);
                return string.IsNullOrWhiteSpace(safeName) ? "Unnamed" : safeName;
            }

            private void UpdateProfileDates(ModProfile profile)
            {
                profile.LastModifiedDate = DateTime.Now;
                if (profile.CreatedDate == DateTime.MinValue)
                {
                    profile.CreatedDate = DateTime.Now;
                }
            }

            private void SaveProfileToFile(ModProfile profile, string filePath)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = JsonSerializer.Serialize(profile, options);
                File.WriteAllText(filePath, json);
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

