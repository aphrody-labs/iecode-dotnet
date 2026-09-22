using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace IEVRModManager.Models
{
    /// <summary>
    /// Represents the application configuration with all user settings and paths.
    /// </summary>
    public class AppConfig : INotifyPropertyChanged
    {
        private string _gamePath = string.Empty;
        private string _cfgBinPath = string.Empty;
        private string _violaCliPath = string.Empty;
        private string _tmpDir = string.Empty;
        private string _selectedCpkName = string.Empty;
        private string _lastKnownPacksSignature = string.Empty;
        private string _lastKnownSteamBuildId = string.Empty;
        private DateTime _vanillaFallbackUntilUtc = DateTime.MinValue;
        private string _theme = "System";
        private string _language = "System";
        private string _lastAppliedProfile = string.Empty;
        private DateTime _lastCpkListCheckUtc = DateTime.MinValue;
        private DateTime _lastAppUpdateCheckUtc = DateTime.MinValue;
        private DateTime _lastModPrefetchUtc = DateTime.MinValue;
        private bool _showTechnicalLogs = false;

        /// <summary>
        /// Gets or sets the path to the game directory.
        /// </summary>
        public string GamePath
        {
            get => _gamePath;
            set { _gamePath = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the path to the CPK list configuration file (cpk_list.cfg.bin).
        /// </summary>
        public string CfgBinPath
        {
            get => _cfgBinPath;
            set { _cfgBinPath = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the path to the Viola CLI executable (violacli.exe).
        /// </summary>
        public string ViolaCliPath
        {
            get => _violaCliPath;
            set { _violaCliPath = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the name of the selected CPK file.
        /// </summary>
        public string SelectedCpkName
        {
            get => _selectedCpkName;
            set { _selectedCpkName = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the temporary directory path.
        /// </summary>
        public string TmpDir
        {
            get => _tmpDir;
            set { _tmpDir = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the last known packs folder signature for change detection.
        /// </summary>
        public string LastKnownPacksSignature
        {
            get => _lastKnownPacksSignature;
            set { _lastKnownPacksSignature = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the last known Steam build ID.
        /// </summary>
        public string LastKnownSteamBuildId
        {
            get => _lastKnownSteamBuildId;
            set { _lastKnownSteamBuildId = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the UTC date until which vanilla fallback is active.
        /// </summary>
        public DateTime VanillaFallbackUntilUtc
        {
            get => _vanillaFallbackUntilUtc;
            set { _vanillaFallbackUntilUtc = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the selected theme name ("Light", "Dark", or "System").
        /// </summary>
        public string Theme
        {
            get => _theme;
            set { _theme = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the selected language code (e.g., "en-US", "es-ES") or "System".
        /// </summary>
        public string Language
        {
            get => _language;
            set { _language = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the name of the last applied mod profile.
        /// </summary>
        public string LastAppliedProfile
        {
            get => _lastAppliedProfile;
            set { _lastAppliedProfile = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the UTC timestamp of the last CPK list check.
        /// </summary>
        public DateTime LastCpkListCheckUtc
        {
            get => _lastCpkListCheckUtc;
            set { _lastCpkListCheckUtc = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the UTC timestamp of the last application update check.
        /// </summary>
        public DateTime LastAppUpdateCheckUtc
        {
            get => _lastAppUpdateCheckUtc;
            set { _lastAppUpdateCheckUtc = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the UTC timestamp of the last mod prefetch operation.
        /// </summary>
        public DateTime LastModPrefetchUtc
        {
            get => _lastModPrefetchUtc;
            set { _lastModPrefetchUtc = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets whether to show technical logs in the UI. When false, only user-friendly logs are shown.
        /// </summary>
        public bool ShowTechnicalLogs
        {
            get => _showTechnicalLogs;
            set { _showTechnicalLogs = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the list of mod data entries.
        /// </summary>
        public List<ModData> Mods { get; set; } = new List<ModData>();

        /// <summary>
        /// Creates a new instance of <see cref="AppConfig"/> with default values.
        /// </summary>
        /// <returns>A new <see cref="AppConfig"/> instance with default values.</returns>
        public static AppConfig Default()
        {
            return new AppConfig
            {
                GamePath = string.Empty,
                CfgBinPath = string.Empty,
                ViolaCliPath = string.Empty,
                SelectedCpkName = string.Empty,
                LastKnownPacksSignature = string.Empty,
                LastKnownSteamBuildId = string.Empty,
                VanillaFallbackUntilUtc = DateTime.MinValue,
                TmpDir = Config.DefaultTmpDir,
                Theme = "System",
                Language = "System",
                LastAppliedProfile = string.Empty,
                LastCpkListCheckUtc = DateTime.MinValue,
                LastAppUpdateCheckUtc = DateTime.MinValue,
                LastModPrefetchUtc = DateTime.MinValue,
                ShowTechnicalLogs = false,
                Mods = new List<ModData>()
            };
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed. If not specified, the caller member name is used.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

