namespace TombLauncher.Core.Utils;

public static class SavegameUtils
{
    public static int GetSlotNumber(string filename)
    {
        return int.Parse(Path.GetExtension(filename).TrimStart('.')) + 1;
    }
}