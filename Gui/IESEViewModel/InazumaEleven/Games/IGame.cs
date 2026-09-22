using IESEViewModel.InazumaEleven.Logic;
using IESEViewModel.Tools;

namespace IESEViewModel.InazumaEleven.Games;

public interface IGame
{
    GameVersion Version { get; }

    string Name { get; }

    int MaximumPlayer { get; }

    Dictionary<uint, Player> Players { get; }

    Dictionary<uint, Move> Moves { get; }

    Dictionary<uint, Avatar> Avatars { get; }

    Dictionary<uint, Item> Items { get; }

    Dictionary<int, Equipment> Equipments { get; set; }

    List<Player> Reserve { get; set; }

    Dictionary<int, Item> Inventory { get; set; }

    SaveInfo SaveInfo { get; set; }

    List<Team> Teams { get; set; }

    void RecruitPlayer(Player player, bool configure);

    Player ChangePlayer(Player selectedPlayer, Player newPlayer, bool keepOldPlayerInformation);

    (int, int, string, bool) Training(Player player, int newStat, int statIndex);

    void UpdateInventory();

    SaveInfo GetSaveInfo();

    List<Team> GetTactics();

    byte[] Save();

    void SaveSaveInfo(BinaryDataWriter writer);
    void SaveTactics(BinaryDataWriter writer);
    void SaveInventory(BinaryDataWriter writer);
    void SavePlayers(BinaryDataWriter writer);
}

