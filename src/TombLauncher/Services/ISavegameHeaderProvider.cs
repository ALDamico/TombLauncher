using System.Collections.Generic;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Savegames.HeaderReaders;

namespace TombLauncher.Services;

public interface ISavegameHeaderProvider
{
    ISavegameHeaderReader GetHeaderReader(GameEngine gameEngine);
}

public class SavegameHeaderProvider : ISavegameHeaderProvider
{
    public SavegameHeaderProvider()
    {
        InitHeaderReaderMap();
    }

    private readonly Dictionary<GameEngine, ISavegameHeaderReader> _headerReaderMap = new Dictionary<GameEngine, ISavegameHeaderReader>();

    private void InitHeaderReaderMap()
    {
        var unsupportedHeaderReader = new SavegameUnsupportedHeaderReader();
        var classicGamesHeaderReader = new SavegameHeaderReader();
        var tr1xHeaderReader = new Tr1xSavegameHeaderReader();

        _headerReaderMap[GameEngine.Unknown] = unsupportedHeaderReader;
        _headerReaderMap[GameEngine.Ten] = unsupportedHeaderReader;
        _headerReaderMap[GameEngine.Tr1x] = tr1xHeaderReader;
        _headerReaderMap[GameEngine.Tr2x] = unsupportedHeaderReader;
        _headerReaderMap[GameEngine.Tomb2Main] = classicGamesHeaderReader;
        _headerReaderMap[GameEngine.TombAti] = classicGamesHeaderReader;
        _headerReaderMap[GameEngine.TombRaider1] = classicGamesHeaderReader;
        _headerReaderMap[GameEngine.TombRaider2] = classicGamesHeaderReader;
        _headerReaderMap[GameEngine.TombRaider3] = classicGamesHeaderReader;
        _headerReaderMap[GameEngine.TombRaider4] = classicGamesHeaderReader;
        _headerReaderMap[GameEngine.TombRaider5] = classicGamesHeaderReader;
        _headerReaderMap[GameEngine.Tomb3CommunityEdition] = classicGamesHeaderReader;
        _headerReaderMap[GameEngine.TombRaider1Dos] = classicGamesHeaderReader;
    }

    public ISavegameHeaderReader GetHeaderReader(GameEngine gameEngine)
    {
        if (_headerReaderMap.TryGetValue(gameEngine, out var reader))
        {
            return reader;
        }

        return new SavegameUnsupportedHeaderReader();
    }
}
