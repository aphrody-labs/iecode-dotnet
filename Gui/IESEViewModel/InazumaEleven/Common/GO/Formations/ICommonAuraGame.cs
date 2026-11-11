using IESEViewModel.InazumaEleven.Logic;

namespace IESEViewModel.InazumaEleven.Common.GO.Formations;

public interface ICommonAuraGame : ICommonMixiMaxGame
{
    static abstract Dictionary<uint, Player> Auras { get; }
}