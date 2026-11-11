namespace IESEViewModel.InazumaEleven.Logic;

public class Avatar
{
    public string Name { get; }

    public bool IsFightingSpirit { get; }

    public int? Level { get; }

    public int? Usage { get; }

    public Avatar(Avatar avatar, int level, int usage)
    {
        Name = avatar.Name;
        IsFightingSpirit = avatar.IsFightingSpirit;
        Level = level;
        Usage = usage;
    }

    public Avatar(string name, bool isFightingSpirit)
    {
        Name = name;
        IsFightingSpirit = isFightingSpirit;
    }

    public override string ToString()
    {
        return Name;
    }
}