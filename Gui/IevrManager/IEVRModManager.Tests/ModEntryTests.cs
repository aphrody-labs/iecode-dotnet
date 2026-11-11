using System;
using System.IO;
using IEVRModManager.Models;
using Xunit;

namespace IEVRModManager.Tests
{
    public class ModEntryTests
    {
        [Fact]
        public void ModEntry_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var modEntry = new ModEntry();

            // Assert
            Assert.Equal(string.Empty, modEntry.Name);
            Assert.Equal(string.Empty, modEntry.Path);
            Assert.True(modEntry.Enabled);
            Assert.Equal(string.Empty, modEntry.DisplayName);
            Assert.Equal(string.Empty, modEntry.Author);
            Assert.Equal(string.Empty, modEntry.ModVersion);
            Assert.Equal(string.Empty, modEntry.GameVersion);
            Assert.Equal(string.Empty, modEntry.ModLink);
        }

        [Fact]
        public void ModEntry_ParameterizedConstructor_SetsAllProperties()
        {
            // Act
            var modEntry = new ModEntry(
                name: "TestMod",
                path: "C:\\Mods",
                enabled: false,
                displayName: "Test Mod Display",
                author: "Test Author",
                modVersion: "1.0.0",
                gameVersion: "1.5.1",
                modLink: "https://example.com/mod"
            );

            // Assert
            Assert.Equal("TestMod", modEntry.Name);
            Assert.Equal("C:\\Mods", modEntry.Path);
            Assert.False(modEntry.Enabled);
            Assert.Equal("Test Mod Display", modEntry.DisplayName);
            Assert.Equal("Test Author", modEntry.Author);
            Assert.Equal("1.0.0", modEntry.ModVersion);
            Assert.Equal("1.5.1", modEntry.GameVersion);
            Assert.Equal("https://example.com/mod", modEntry.ModLink);
        }

        [Fact]
        public void ModEntry_ParameterizedConstructorWithDefaults_UsesDefaults()
        {
            // Act
            var modEntry = new ModEntry("TestMod", "C:\\Mods");

            // Assert
            Assert.Equal("TestMod", modEntry.Name);
            Assert.Equal("C:\\Mods", modEntry.Path);
            Assert.True(modEntry.Enabled);
            Assert.Equal("TestMod", modEntry.DisplayName); // Uses name as default
            Assert.Equal(string.Empty, modEntry.Author);
            Assert.Equal(string.Empty, modEntry.ModVersion);
        }

        [Fact]
        public void ModEntry_FullPath_CombinesPathAndName()
        {
            // Arrange
            var modEntry = new ModEntry("TestMod", "C:\\Mods");

            // Act
            var fullPath = modEntry.FullPath;

            // Assert
            Assert.Equal(Path.Combine("C:\\Mods", "TestMod"), fullPath);
        }

        [Fact]
        public void ModEntry_FullPath_WithTrailingSlash_HandlesCorrectly()
        {
            // Arrange
            var modEntry = new ModEntry("TestMod", "C:\\Mods\\");

            // Act
            var fullPath = modEntry.FullPath;

            // Assert
            Assert.Equal(Path.Combine("C:\\Mods", "TestMod"), fullPath);
        }

        [Fact]
        public void ModEntry_ToData_ConvertsCorrectly()
        {
            // Arrange
            var modEntry = new ModEntry(
                name: "TestMod",
                path: "C:\\Mods",
                enabled: false,
                modLink: "https://example.com/mod"
            );

            // Act
            var modData = modEntry.ToData();

            // Assert
            Assert.Equal("TestMod", modData.Name);
            Assert.False(modData.Enabled);
            Assert.Equal("https://example.com/mod", modData.ModLink);
        }

        [Fact]
        public void ModEntry_ToData_PreservesEnabledState()
        {
            // Arrange
            var modEntry = new ModEntry("TestMod", "C:\\Mods", enabled: true);
            var modEntry2 = new ModEntry("TestMod2", "C:\\Mods", enabled: false);

            // Act
            var data1 = modEntry.ToData();
            var data2 = modEntry2.ToData();

            // Assert
            Assert.True(data1.Enabled);
            Assert.False(data2.Enabled);
        }

        [Fact]
        public void ModEntry_FromData_CreatesCorrectly()
        {
            // Arrange
            var modData = new ModData
            {
                Name = "TestMod",
                Enabled = false,
                ModLink = "https://example.com/mod"
            };
            var basePath = "C:\\Mods";

            // Act
            var modEntry = ModEntry.FromData(modData, basePath);

            // Assert
            Assert.Equal("TestMod", modEntry.Name);
            Assert.Equal(basePath, modEntry.Path);
            Assert.False(modEntry.Enabled);
            Assert.Equal("https://example.com/mod", modEntry.ModLink);
        }

        [Fact]
        public void ModEntry_FromData_WithNullModLink_HandlesCorrectly()
        {
            // Arrange
            var modData = new ModData
            {
                Name = "TestMod",
                Enabled = true,
                ModLink = null
            };

            // Act
            var modEntry = ModEntry.FromData(modData, "C:\\Mods");

            // Assert
            Assert.Equal(string.Empty, modEntry.ModLink);
        }

        [Fact]
        public void ModData_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var modData = new ModData();

            // Assert
            Assert.Equal(string.Empty, modData.Name);
            Assert.True(modData.Enabled);
            Assert.Null(modData.ModLink);
        }

        [Fact]
        public void ModEntry_WithEmptyName_FullPathStillWorks()
        {
            // Arrange
            var modEntry = new ModEntry
            {
                Name = "",
                Path = "C:\\Mods"
            };

            // Act
            var fullPath = modEntry.FullPath;

            // Assert
            Assert.Equal("C:\\Mods", fullPath);
        }

        [Fact]
        public void ModEntry_WithEmptyPath_FullPathStillWorks()
        {
            // Arrange
            var modEntry = new ModEntry
            {
                Name = "TestMod",
                Path = ""
            };

            // Act
            var fullPath = modEntry.FullPath;

            // Assert
            Assert.Equal("TestMod", fullPath);
        }
    }
}
