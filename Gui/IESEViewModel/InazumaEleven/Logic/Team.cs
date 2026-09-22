namespace IESEViewModel.InazumaEleven.Logic;

public class Team(
    string name,
    Item? emblem,
    Item? kit,
    Item? formation,
    Item? coach,
    List<Player?> players,
    List<int> playersFormationIndex,
    List<int> playersKitNumber)
{
    public readonly string Name = name;

    // Unused
    public int? Level;

    public readonly Item? Emblem = emblem;

    public readonly Item? Kit = kit;

    public readonly Item? Formation = formation;

    public readonly Item? Coach = coach;

    public readonly List<Player?> Players = players;

    public readonly List<int> PlayersFormationIndex = playersFormationIndex;

    public readonly List<int> PlayersKitNumber = playersKitNumber;
}