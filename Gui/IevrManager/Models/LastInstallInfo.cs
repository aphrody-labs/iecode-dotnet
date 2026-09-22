using System;
using System.Collections.Generic;

namespace IEVRModManager.Models
{
    /// <summary>
    /// Represents information about the last mod installation that was applied.
    /// </summary>
    public class LastInstallInfo
    {
        /// <summary>
        /// Gets or sets the game path where mods were installed.
        /// </summary>
        public string GamePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the list of file paths that were installed.
        /// </summary>
        public List<string> Files { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the list of mod names that were installed.
        /// </summary>
        public List<string> Mods { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the date and time when the installation was applied.
        /// </summary>
        public DateTime AppliedAt { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// Gets or sets the selected CPK file name used during installation.
        /// </summary>
        public string SelectedCpkName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets additional information about the selected CPK file.
        /// </summary>
        public string SelectedCpkInfo { get; set; } = string.Empty;

        /// <summary>
        /// Creates an empty <see cref="LastInstallInfo"/> instance.
        /// </summary>
        /// <returns>A new <see cref="LastInstallInfo"/> instance with empty/default values.</returns>
        public static LastInstallInfo Empty()
        {
            return new LastInstallInfo
            {
                GamePath = string.Empty,
                Files = new List<string>(),
                Mods = new List<string>(),
                AppliedAt = DateTime.MinValue,
                SelectedCpkName = string.Empty,
                SelectedCpkInfo = string.Empty
            };
        }
    }
}
