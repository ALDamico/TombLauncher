using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Utils;

namespace TombLauncher.Core.Savegames.HeaderReaders;

public class SavegameHeaderReader : ISavegameHeaderReader
{
    public SavegameHeaderReader()
    {
        
    }
    private const int MaxLevelNameLength = 75;
    private const int SaveNumberOffset = 75;
    public SavegameHeader ReadHeader(string filepath)
    {
        using var fs = File.OpenRead(filepath);
        var buf = new byte[80];
        var bytesRead = fs.Read(buf, 0, buf.Length);
        if (bytesRead < 80)
            return null;

        return ReadHeader(filepath, buf);
    }

    public SavegameHeader ReadHeader(string filepath, byte[] buf)
    {
        if (buf.Length < 80)
            return null;
        var levelName = StringExtensions.GetNullTerminatedString(buf, MaxLevelNameLength).Replace('*', ' ');
        var saveNo = BitConverter.ToInt32(buf, SaveNumberOffset);
        var slotNumber = SavegameUtils.GetSlotNumber(filepath);
        return new SavegameHeader()
        {
            Filepath = filepath,
            LevelName = levelName,
            SaveNumber = saveNo,
            SlotNumber = slotNumber
        };
    }
}