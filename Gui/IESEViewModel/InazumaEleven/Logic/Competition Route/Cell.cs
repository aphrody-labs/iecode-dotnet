namespace IESEViewModel.InazumaEleven.Logic.Competition_Route;

public class Cell(string name, int level, List<uint> drop)
{
    public string Name { get; set; } = name;

    public int Level { get; set; } = level;

    public List<uint> Drop { get; set; } = drop;
}
