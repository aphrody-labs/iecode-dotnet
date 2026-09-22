namespace IESEViewModel.InazumaEleven.Logic;

public class Element(string name)
{
    public readonly string Name = name;

    public Element? VeryEffective;

    public Element? NotVeryEffective;

    public override string ToString()
    {
        return Name;
    }

    public string? Advantage()
    {
        return this == Void() ? null : VeryEffective!.ToString();
    }

    public string? Disadvantage()
    {
        return this == Void() ? null : NotVeryEffective!.ToString();
    }

    public static Element Earth()
    {
        Element earth = new Element("Earth")
        {
            VeryEffective = Fire(),
            NotVeryEffective = Wind()
        };
        return earth;
    }

    public static Element Fire()
    {
        Element fire = new Element("Fire")
        {
            VeryEffective = new Element("Wood"),
            NotVeryEffective = new Element("Earth")
        };
        return fire;
    }

    public static Element Wind()
    {
        Element wind = new Element("Wind")
        {
            VeryEffective = new Element("Earth"),
            NotVeryEffective = new Element("Wood")
        };
        return wind;
    }

    public static Element Wood()
    {
        Element wood = new Element("Wood")
        {
            VeryEffective = new Element("Wind"),
            NotVeryEffective = new Element("Fire")
        };
        return wood;
    }

    public static Element Void()
    {
        Element voidElement = new Element("Void")
        {
            VeryEffective = null,
            NotVeryEffective = null
        };
        return voidElement;
    }
}