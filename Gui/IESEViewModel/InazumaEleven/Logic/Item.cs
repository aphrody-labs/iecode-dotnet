namespace IESEViewModel.InazumaEleven.Logic;

public class Item
{
    public readonly string? Name;

    public int Quantity;

    public int QuantityEquiped;

    public readonly string? IndexSorted;

    public readonly int Category;

    public readonly int SubCategory;

    public readonly uint Id;

    public Item()
    {

    }

    public Item(uint id, Item item)
    {
        Id = id;
        Name = item.Name;
        Quantity = item.Quantity;
        QuantityEquiped = item.QuantityEquiped;
        IndexSorted = item.IndexSorted;
        SubCategory = item.SubCategory;
        Category = item.Category;
    }

    public Item(string name, int category, int subCategory)
    {
        Name = name;
        Category = category;
        SubCategory = subCategory;
    }

    public Item(string indexSorted, int quantity)
    {
        IndexSorted = indexSorted;
        Quantity = quantity;
    }

    public Item(uint id, Item item, int quantity)
    {
        Id = id;
        Name = item.Name;
        Category = item.Category;
        SubCategory = item.SubCategory;
        Quantity = quantity;
    }

    public Item(uint id, Item item, int quantity, int quantityEquiped)
    {
        Id = id;
        Name = item.Name;
        Category = item.Category;
        SubCategory = item.SubCategory;
        Quantity = quantity;
        QuantityEquiped = quantityEquiped;
    }

    public override string? ToString()
    {
        return Name;
    }
}