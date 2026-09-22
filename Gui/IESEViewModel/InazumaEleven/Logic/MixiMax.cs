namespace IESEViewModel.InazumaEleven.Logic;

public class MixiMax
{
    public readonly Player? AuraPlayer;

    public bool AuraData;

    public BestMatch? BestMatch;

    public readonly List<int> MixiMaxMoveNumber = [];

    public MixiMax()
    {

    }

    public MixiMax(Player auraPlayer, (int, int) mixiMaxMove)
    {
        AuraPlayer = auraPlayer;
        MixiMaxMoveNumber.Add(mixiMaxMove.Item1);
        MixiMaxMoveNumber.Add(mixiMaxMove.Item2);
    }

    public MixiMax(Player auraPlayer, (int, int) mixiMaxMove, BestMatch? bestMatch)
    {
        AuraPlayer = auraPlayer;
        MixiMaxMoveNumber.Add(mixiMaxMove.Item1 == 6 ? 6 : mixiMaxMove.Item1);
        MixiMaxMoveNumber.Add(mixiMaxMove.Item2 == 6 ? 6 : mixiMaxMove.Item2);
        BestMatch = bestMatch;
    }
}
