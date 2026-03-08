using TombLauncher.Core.Dtos;

namespace TombLauncher.Core.Savegames.HeaderReaders;

public interface ISavegameHeaderReader
{
    SavegameHeader ReadHeader(string filepath);
    SavegameHeader ReadHeader(string filepath, byte[] buf);
}