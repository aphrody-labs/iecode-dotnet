using IESEViewModel.InazumaEleven.Common.GO;
using IESEViewModel.InazumaEleven.Logic;
using IESEViewModel.Tools;

namespace IESEViewModel.InazumaEleven.Games;

public abstract class GameMiximax<TCommon>(BinaryDataReader data, GameVersion version, string name, int maximumPlayer) : Game<TCommon>(data, version, name, maximumPlayer), IGameMixiMax where TCommon : ICommonMixiMaxGame
{
    private Dictionary<int, List<PlayRecord>>? _playRecords;

    public Dictionary<int, List<PlayRecord>> PlayRecords
    {
        get => _playRecords ??= GetPlayRecords();
        set => _playRecords = value;
    }

    public Dictionary<int, Player> Auras { get; set; } = null!; // must be set in inheritors

    protected bool PlayRecordsInitialized => _playRecords != null;

    public abstract Dictionary<int, List<PlayRecord>> GetPlayRecords();

    public abstract void SavePlayRecords(BinaryDataWriter writer);

    protected BestMatch? IsBestMatch(uint tryPlayerId, uint tryAuraId)
    {
        List<BestMatch> compatibleBestMatchs = TCommon.BestMatches.Where(x => x.PlayerId == tryPlayerId).ToList();

        if (compatibleBestMatchs.Count == -1) return null;

        BestMatch? bestMatch =
            compatibleBestMatchs.FirstOrDefault(x => x.PlayerId == tryPlayerId && x.AuraId == tryAuraId) ??
            // Player who can perform a miximax with everyone
            compatibleBestMatchs.FirstOrDefault(x => x.PlayerId == tryPlayerId && x.AuraId == 0x0);

        return bestMatch;
    }

    public override byte[] Save()
    {
        byte[] saveData = base.Save();
        BinaryDataWriter dataWriter = new BinaryDataWriter(saveData);
        SavePlayRecords(dataWriter);
        return saveData;
    }
}