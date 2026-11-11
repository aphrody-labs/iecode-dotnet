using IESEViewModel.InazumaEleven.Logic;
using IESEViewModel.Tools;

namespace IESEViewModel.InazumaEleven.Games;

public interface IGameMixiMax : IGame
{
    Dictionary<int, List<PlayRecord>> GetPlayRecords();

    Dictionary<int, Player> Auras { get; set; }

    void SavePlayRecords(BinaryDataWriter writer);
}