namespace IESEViewModel.InazumaEleven.Logic;

public class Position(string name)
{
    public readonly string Name = name;

    public override string ToString()
    {
        return Name;
    }

    public static Position None()
    {
        return new Position("None");
    }

    public static Position Goalkeeper()
    {
        return new Position("Goalkeeper");
    }

    public static Position Forward()
    {
        return new Position("Forward");
    }

    public static Position Midfielder()
    {
        return new Position("Midfielder");
    }

    public static Position Defender()
    {
        return new Position("Defender");
    }

    public static Position Save()
    {
        return new Position("Save");
    }

    public static Position Shot()
    {
        return new Position("Shot");
    }

    public static Position Dribble()
    {
        return new Position("Dribble");
    }

    public static Position Block()
    {
        return new Position("Block");
    }
}