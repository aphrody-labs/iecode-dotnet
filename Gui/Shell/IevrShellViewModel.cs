using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using IEVRModManager;
using IEVRModManager.Helpers;
using IEVRModManager.Managers;
using IEVRModManager.Models;

namespace IECODE.Gui.Shell;

/// <summary>
/// Avalonia-facing adapter for the existing IEVR configuration, profile, mod and Steam services.
/// It deliberately owns no WPF types and does not create a second application host.
/// </summary>
public sealed class IevrShellViewModel : INotifyPropertyChanged
{
    private readonly ConfigManager _configManager = new();
    private readonly ProfileManager _profileManager = new();
    private readonly ModManager _modManager = new();
    private AppConfig _config = AppConfig.Default();
    private ModProfile? _selectedProfile;
    private string _gamePath = string.Empty;
    private string _profileName = "Default";
    private string _status = "IEVR shell ready.";
    private string _steamStatus = "Steam detection has not run.";

    public IevrShellViewModel()
    {
        RefreshCommand = new ShellDelegateCommand(Refresh);
        DetectSteamCommand = new ShellDelegateCommand(DetectSteam);
        SaveConfigurationCommand = new ShellDelegateCommand(SaveConfiguration);
        SaveProfileCommand = new ShellDelegateCommand(SaveProfile);
        ApplyProfileCommand = new ShellDelegateCommand(ApplySelectedProfile, () => SelectedProfile is not null);
        DeleteProfileCommand = new ShellDelegateCommand(DeleteSelectedProfile, () => SelectedProfile is not null);

        Refresh();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ModEntry> Mods { get; } = [];

    public ObservableCollection<ModProfile> Profiles { get; } = [];

    public ShellDelegateCommand RefreshCommand { get; }

    public ShellDelegateCommand DetectSteamCommand { get; }

    public ShellDelegateCommand SaveConfigurationCommand { get; }

    public ShellDelegateCommand SaveProfileCommand { get; }

    public ShellDelegateCommand ApplyProfileCommand { get; }

    public ShellDelegateCommand DeleteProfileCommand { get; }

    public ModProfile? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (ReferenceEquals(_selectedProfile, value))
            {
                return;
            }

            _selectedProfile = value;
            OnPropertyChanged();
            ApplyProfileCommand.NotifyCanExecuteChanged();
            DeleteProfileCommand.NotifyCanExecuteChanged();
        }
    }

    public string GamePath
    {
        get => _gamePath;
        set
        {
            if (string.Equals(_gamePath, value, StringComparison.Ordinal))
            {
                return;
            }

            _gamePath = value;
            OnPropertyChanged();
        }
    }

    public string ProfileName
    {
        get => _profileName;
        set
        {
            if (string.Equals(_profileName, value, StringComparison.Ordinal))
            {
                return;
            }

            _profileName = value;
            OnPropertyChanged();
        }
    }

    public string ModsDirectory => Config.DefaultModsDir;

    public string Status
    {
        get => _status;
        private set
        {
            if (string.Equals(_status, value, StringComparison.Ordinal))
            {
                return;
            }

            _status = value;
            OnPropertyChanged();
        }
    }

    public string SteamStatus
    {
        get => _steamStatus;
        private set
        {
            if (string.Equals(_steamStatus, value, StringComparison.Ordinal))
            {
                return;
            }

            _steamStatus = value;
            OnPropertyChanged();
        }
    }

    public string ModSummary => $"{Mods.Count} mods, {Mods.Count(mod => mod.Enabled)} enabled";

    public void Refresh()
    {
        try
        {
            _config = _configManager.Load();
            GamePath = _config.GamePath;

            ReplaceMods(_modManager.ScanMods(_config.Mods));
            ReplaceProfiles(_profileManager.GetAllProfiles());
            Status = $"Loaded {Mods.Count} mods and {Profiles.Count} profiles.";
            OnPropertyChanged(nameof(ModSummary));
        }
        catch (Exception exception)
        {
            Status = $"Unable to load IEVR data: {exception.Message}";
        }
    }

    public void DetectSteam()
    {
        try
        {
            var detectedPath = SteamHelper.DetectGamePath();
            if (string.IsNullOrWhiteSpace(detectedPath))
            {
                SteamStatus = OperatingSystem.IsWindows()
                    ? "Steam game installation not found."
                    : "Steam registry detection is available on Windows only.";
                return;
            }

            GamePath = detectedPath;
            _config.GamePath = detectedPath;
            SteamStatus = $"Steam game found: {detectedPath}";
            Status = "Steam game path detected.";
        }
        catch (Exception exception)
        {
            SteamStatus = $"Steam detection failed: {exception.Message}";
        }
    }

    public void SaveConfiguration()
    {
        try
        {
            _config.GamePath = GamePath.Trim();
            _config.Mods = Mods.Select(mod => mod.ToData()).ToList();
            _config.LastAppliedProfile = SelectedProfile?.Name ?? _config.LastAppliedProfile;

            if (_configManager.Save(_config))
            {
                Status = "IEVR configuration saved.";
            }
            else
            {
                Status = "IEVR configuration could not be saved.";
            }
        }
        catch (Exception exception)
        {
            Status = $"Configuration save failed: {exception.Message}";
        }
    }

    public void SaveProfile()
    {
        var name = ProfileName.Trim();
        if (name.Length == 0)
        {
            Status = "Enter a profile name first.";
            return;
        }

        try
        {
            var profile = new ModProfile
            {
                Name = name,
                SelectedCpkName = _config.SelectedCpkName,
                Mods = Mods.Select(mod => mod.ToData()).ToList()
            };

            if (!_profileManager.SaveProfile(profile))
            {
                Status = "Profile could not be saved.";
                return;
            }

            ReplaceProfiles(_profileManager.GetAllProfiles());
            SelectedProfile = Profiles.FirstOrDefault(profileItem =>
                string.Equals(profileItem.Name, name, StringComparison.OrdinalIgnoreCase));
            Status = $"Profile '{name}' saved.";
        }
        catch (Exception exception)
        {
            Status = $"Profile save failed: {exception.Message}";
        }
    }

    public void ApplySelectedProfile()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        var profileMods = SelectedProfile.Mods.ToDictionary(
            mod => mod.Name,
            mod => mod.Enabled,
            StringComparer.OrdinalIgnoreCase);

        foreach (var mod in Mods)
        {
            if (profileMods.TryGetValue(mod.Name, out var enabled))
            {
                mod.Enabled = enabled;
            }
        }

        _config.SelectedCpkName = SelectedProfile.SelectedCpkName;
        _config.LastAppliedProfile = SelectedProfile.Name;
        OnPropertyChanged(nameof(ModSummary));
        Status = $"Profile '{SelectedProfile.Name}' applied.";
    }

    public void DeleteSelectedProfile()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        var profileName = SelectedProfile.Name;
        try
        {
            if (_profileManager.DeleteProfile(profileName))
            {
                ReplaceProfiles(_profileManager.GetAllProfiles());
                Status = $"Profile '{profileName}' deleted.";
            }
            else
            {
                Status = $"Profile '{profileName}' was not found.";
            }
        }
        catch (Exception exception)
        {
            Status = $"Profile deletion failed: {exception.Message}";
        }
    }

    private void ReplaceMods(IEnumerable<ModEntry> entries)
    {
        Mods.Clear();
        foreach (var entry in entries)
        {
            Mods.Add(entry);
        }

        OnPropertyChanged(nameof(ModSummary));
    }

    private void ReplaceProfiles(IEnumerable<ModProfile> profiles)
    {
        Profiles.Clear();
        foreach (var profile in profiles)
        {
            Profiles.Add(profile);
        }

        SelectedProfile = Profiles.FirstOrDefault(profile =>
            string.Equals(profile.Name, _config.LastAppliedProfile, StringComparison.OrdinalIgnoreCase));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
