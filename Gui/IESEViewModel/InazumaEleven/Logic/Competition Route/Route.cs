namespace IESEViewModel.InazumaEleven.Logic.Competition_Route;

public class Route(string name, List<Cell> cells)
{
    public string Name { get; set; } = name;

    public List<Cell> Cells { get; set; } = cells;
}