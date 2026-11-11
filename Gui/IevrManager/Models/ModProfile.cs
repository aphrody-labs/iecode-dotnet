using System;
using System.Collections.Generic;

namespace IEVRModManager.Models
{
    /// <summary>
    /// Represents a mod profile that saves a specific mod configuration.
    /// </summary>
    public class ModProfile
    {
        /// <summary>
        /// Gets or sets the name of the profile.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the date and time when the profile was created.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Gets or sets the date and time when the profile was last modified.
        /// </summary>
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Gets or sets the list of mod data entries in this profile.
        /// </summary>
        public List<ModData> Mods { get; set; } = new List<ModData>();
        
        /// <summary>
        /// Gets or sets the selected CPK file name for this profile.
        /// </summary>
        public string SelectedCpkName { get; set; } = string.Empty;
    }
}

