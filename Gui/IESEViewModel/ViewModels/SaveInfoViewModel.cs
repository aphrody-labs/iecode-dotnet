using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace IESEViewModel.ViewModels;

public class SaveInfoViewModel(int hours, int minutes, int currentChapter, string teamName, string userName, int prestige, int goVersion, int? linkLevel = null, int? friendship = null, int[]? coins = null) : ObservableValidator
{
    private int _currentChapter = currentChapter;
    private int _minutes = minutes;
    private string _teamName = teamName;
    private string _userName = userName;
    private int _prestige = prestige;
    private int? _linkLevel = linkLevel;
    private int? _friendship = friendship;
    private Dictionary<string, int>? _coins = coins?.Select((amount, index) => KeyValuePair.Create($"avares://IESE/Assets/coin{index}.png", amount)).ToDictionary();
    private bool _unlockLink;
    private bool _unlockAll;
    public int Hours { get; set; } = hours;

    public int Minutes
    {
        get => _minutes;
        set => SetProperty(ref _minutes, value);
    }

    public int CurrentChapter
    {
        get => _currentChapter;
        set => SetProperty(ref _currentChapter, value);
    }

    public string TeamName
    {
        get => _teamName;
        set => SetProperty(ref _teamName, value);
    }

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public int Prestige
    {
        get => _prestige;
        set => SetProperty(ref _prestige, value);
    }

    public Dictionary<string, int>? Coins
    {
        get => _coins;
        set => SetProperty(ref _coins, value);
    }

    private int GoVersion { get; } = goVersion;

    [Required]
    public int? LinkLevel
    {
        get => _linkLevel;
        set => SetProperty(ref _linkLevel, value, true);
    }

    public int? Friendship
    {
        get => _friendship;
        set => SetProperty(ref _friendship, value);
    }

    public bool UnlockLink
    {
        get => _unlockLink;
        set => SetProperty(ref _unlockLink, value);
    }

    public bool UnlockAll
    {
        get => _unlockAll;
        set => SetProperty(ref _unlockAll, value);
    }

    public bool IsGo => GoVersion == 1;

    public bool IsGalaxy => GoVersion == 3;

    // Only for previewer in an IDE
    public SaveInfoViewModel() : this(0, 0, 0, "", "", 0, 3, 0, coins: [10, 15, 20, 25, 30])
    { }
}
