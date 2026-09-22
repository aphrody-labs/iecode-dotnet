namespace IESEViewModel.InazumaEleven.Logic;

public class Gender(string name)
{
    public string Name = name;

    public static Gender Boy()
    {
        return new Gender("Boy");
    }

    public static Gender Girl()
    {
        return new Gender("Girl");
    }

    public static Gender Unknown()
    {
        return new Gender("Unknown");
    }

    public override string ToString()
    {
        return Name;
    }
}