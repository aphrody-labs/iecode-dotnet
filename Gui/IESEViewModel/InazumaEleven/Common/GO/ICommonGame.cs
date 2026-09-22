using IESEViewModel.InazumaEleven.Logic;

namespace IESEViewModel.InazumaEleven.Common.GO;

public interface ICommonGame
{
    static abstract Dictionary<uint, Avatar> Avatars { get; }
    static abstract Dictionary<uint, Equipment> Equipments { get; }
    static abstract Dictionary<uint, Item> Items { get; }
    static abstract Dictionary<uint, Move> Moves { get; }
    static abstract Dictionary<uint, Player> Players { get; }
}