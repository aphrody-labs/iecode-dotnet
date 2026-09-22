using System;
using System.IO;
using System.Text.Json.Serialization;

namespace IEVRModManager.Models
{
    /// <summary>
    /// Represents a mod entry with metadata and state information.
    /// </summary>
    public class ModEntry
    {
        /// <summary>
        /// Gets or sets the mod directory name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the base path where the mod is located.
        /// </summary>
        public string Path { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets whether the mod is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the display name of the mod (from metadata).
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the author of the mod (from metadata).
        /// </summary>
        public string Author { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the mod version (from metadata).
        /// </summary>
        public string ModVersion { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the game version this mod targets (from metadata).
        /// </summary>
        public string GameVersion { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the link to the mod (from metadata).
        /// </summary>
        public string ModLink { get; set; } = string.Empty;

        /// <summary>
        /// Gets the full path to the mod directory.
        /// </summary>
        [JsonIgnore]
        public string FullPath => System.IO.Path.Combine(Path, Name);

        /// <summary>
        /// Initializes a new instance of the <see cref="ModEntry"/> class.
        /// </summary>
        public ModEntry()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModEntry"/> class with specified values.
        /// </summary>
        /// <param name="name">The mod directory name.</param>
        /// <param name="path">The base path where the mod is located.</param>
        /// <param name="enabled">Whether the mod is enabled.</param>
        /// <param name="displayName">The display name of the mod.</param>
        /// <param name="author">The author of the mod.</param>
        /// <param name="modVersion">The mod version.</param>
        /// <param name="gameVersion">The game version this mod targets.</param>
        /// <param name="modLink">The link to the mod.</param>
        public ModEntry(string name, string path, bool enabled = true, 
            string? displayName = null, string? author = null, 
            string? modVersion = null, string? gameVersion = null,
            string? modLink = null)
        {
            Name = name;
            Path = path;
            Enabled = enabled;
            DisplayName = displayName ?? name;
            Author = author ?? string.Empty;
            ModVersion = modVersion ?? string.Empty;
            GameVersion = gameVersion ?? string.Empty;
            ModLink = modLink ?? string.Empty;
        }

        /// <summary>
        /// Converts this mod entry to a <see cref="ModData"/> object for serialization.
        /// </summary>
        /// <returns>A <see cref="ModData"/> object containing the essential mod information.</returns>
        public ModData ToData()
        {
            return new ModData
            {
                Name = Name,
                Enabled = Enabled,
                ModLink = ModLink
            };
        }

        /// <summary>
        /// Creates a <see cref="ModEntry"/> from a <see cref="ModData"/> object.
        /// </summary>
        /// <param name="data">The mod data to convert.</param>
        /// <param name="basePath">The base path where the mod is located.</param>
        /// <returns>A new <see cref="ModEntry"/> instance.</returns>
        public static ModEntry FromData(ModData data, string basePath)
        {
            return new ModEntry
            {
                Name = data.Name,
                Path = basePath,
                Enabled = data.Enabled,
                ModLink = data.ModLink ?? string.Empty
            };
        }
    }

    /// <summary>
    /// Represents minimal mod data for serialization purposes.
    /// </summary>
    public class ModData
    {
        /// <summary>
        /// Gets or sets the mod directory name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets whether the mod is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the link to the mod.
        /// </summary>
        public string? ModLink { get; set; }
    }
}

