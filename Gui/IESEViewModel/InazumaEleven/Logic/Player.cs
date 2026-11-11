namespace IESEViewModel.InazumaEleven.Logic;

public class Player
{
    public readonly string? Name;

    public uint Id;

    public int Index;

    public readonly Element? Element;

    public readonly Position? Position;

    public readonly Gender? Gender;

    public readonly List<int>? Stat;

    public readonly int Freedom;

    public List<Move>? Moves;

    public readonly List<uint>? UInt32Moves;

    public int Level;

    public bool Invoke;

    public bool Armed;

    public int Style;

    public List<int>? InvestedPoint;

    public List<int>? InvestedFreedom;

    public Avatar? Avatar;

    public List<Equipment>? Equipments;

    public MixiMax? MixiMax;

    public bool IsAura;

    public bool IsTrained;

    public int Score;

    public int Participation;

    public int Fp;

    public int Tp;

    public bool IsKeyPlayer;

    public Player()
    {

    }

    public Player(Player player, uint? id = null)
    {
        Name = player.Name;
        Id = id ?? player.Id;
        Index = player.Index;
        Element = player.Element;
        Position = player.Position;
        Gender = player.Gender;
        Stat = player.Stat;
        Freedom = player.Freedom;
        Moves = player.Moves;
        UInt32Moves = player.UInt32Moves;
        Level = player.Level;
        Invoke = player.Invoke;
        Armed = player.Armed;
        Style = player.Style;
        InvestedPoint = player.InvestedPoint;
        InvestedFreedom = player.InvestedFreedom;
        Avatar = player.Avatar;
        Equipments = player.Equipments;
        MixiMax = player.MixiMax;
        IsAura = player.IsAura;
        IsTrained = player.IsTrained;
        Score = player.Score;
        Participation = player.Participation;
        Fp = player.Fp;
        Tp = player.Tp;
    }

    public Player(string name, Position? position, Element? element, Gender? gender, List<uint>? moves, List<int>? stat, int freedom)
    {
        Name = name;
        Position = position;
        Element = element;
        Gender = gender;
        UInt32Moves = moves;
        Stat = stat;
        Freedom = freedom;
        InvestedFreedom = new List<int>(new int[8]);
    }

    public override string? ToString()
    {
        return Name;
    }
}
