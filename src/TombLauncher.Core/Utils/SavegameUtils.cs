namespace TombLauncher.Core.Utils;

public static class SavegameUtils
{
    public static int GetSlotNumber(string filename)
    {
        var parsed = int.TryParse(Path.GetExtension(filename).TrimStart('.'), out var result);
        return parsed ? result + 1 : -1;
    }

    public static int GetTr1xSlotNumber(string filename)
    {
        var parsed = int.TryParse(filename.Split("_").LastOrDefault()?.Replace(".dat", ""), out var result);
        if (!parsed)
            return -1;

        return result;
    }
}