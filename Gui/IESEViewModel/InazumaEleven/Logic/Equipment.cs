namespace IESEViewModel.InazumaEleven.Logic;

public class Equipment(string name, EquipmentType type, List<int> stat)
{
    public readonly string Name = name;

    public uint Id;

    public EquipmentType Type = type;

    public List<int> Stat = stat;

    public override string ToString()
    {
        return Name;
    }
}

public class EquipmentType(string name)
{
    public string Name = name;
}
