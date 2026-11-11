namespace IESEViewModel.InazumaEleven.Games;

public enum GameVersion
{
    Go = 4,
    Cs = 5,
    Galaxy = 6
}

public static class GameVersionHelper
{
    public static string GetFileEndingForVersion(GameVersion version)
    {
        return version switch
        {
            GameVersion.Go => ".ie4",
            GameVersion.Cs or GameVersion.Galaxy => ".ie",
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };
    }

    public static string[] GetAllFileEndings()
    {
        return ["*.ie4", "*.ie"];
    }
}