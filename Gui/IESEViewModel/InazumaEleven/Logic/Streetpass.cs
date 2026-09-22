namespace IESEViewModel.InazumaEleven.Logic;

public class Streetpass(
    string name,
    string message,
    string gameVersion,
    bool hasWon,
    int playTime,
    int challengesReceived,
    int challengesWon,
    int playersScouted,
    Team team)
{
    public string? Name = name;

    public string? Message = message;

    public string? GameVersion = gameVersion;

    public bool HasWon = hasWon;

    public int PlayTime = playTime;

    public int ChallengesReceived = challengesReceived;

    public int ChallengesWon = challengesWon;

    public int PlayersScouted = playersScouted;

    public Team? Team = team;
}