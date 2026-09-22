namespace IESEViewModel.InazumaEleven.Logic;

public class SaveInfo(
    string name,
    string teamName,
    int hours,
    int minutes,
    int prestige,
    int friendship,
    byte? secretLinkLevel = null,
    byte? chapter = null,
    int[]? coins = null)
{
    public string Name = name;

    public string TeamName = teamName;

    public int? Chapter = chapter;

    public int Hours = hours;

    public int Minutes = minutes;

    public int Prestige = prestige;

    public int Friendship = friendship;

    public int? SecretLinkLevel = secretLinkLevel;

    public readonly int[]? Coins = coins;

    public bool UnlockAllData;

    public Dictionary<int, List<PlayRecord>>? PlayRecords;

    public Dictionary<uint, Item> Inventory = new();

    public List<Team>? Teams;
    public bool UnlockLinkLevel;
}
