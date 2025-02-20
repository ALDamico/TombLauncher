using TombLauncher.Core.Dtos;
using TombLauncher.Core.Utils;

namespace TombLauncher.Core.Savegames.HeaderReaders;

public class SavegameUnsupportedHeaderReader : ISavegameHeaderReader
{
    public SavegameHeader ReadHeader(string filepath)
    {
        return new SavegameHeader()
        {
            Filepath = filepath,
            SaveNumber = -1,
            SlotNumber = SavegameUtils.GetSlotNumber(filepath)
        };
    }

    public SavegameHeader ReadHeader(string filepath, byte[] buf)
    {
        return ReadHeader(filepath);
    }
}