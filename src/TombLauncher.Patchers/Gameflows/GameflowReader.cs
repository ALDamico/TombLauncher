using Microsoft.Extensions.Logging;
using TombLauncher.Core.Extensions;

namespace TombLauncher.Patchers.Gameflows;

public class GameflowReader
{
    private readonly ILogger<GameflowReader> _logger;

    public GameflowReader(ILogger<GameflowReader> logger)
    {
        _logger = logger;
    }
    public Gameflow ReadGameflow(string path)
    {
        var gameflow = new Gameflow();
        using var fs = File.OpenRead(path);
        using var sr = new BinaryReader(fs);
        ReadVersion(sr, gameflow);
        ReadDescription(sr, gameflow);
        ReadGameflowSize(sr, gameflow);
        ReadFirstOption(sr, gameflow);
        ReadTitleReplace(sr, gameflow);
        ReadOnDeathDemoMode(sr, gameflow);
        ReadOnDeathInGame(sr, gameflow);
        ReadDemoTime(sr, gameflow);
        ReadOnDemoInterrupt(sr, gameflow);
        ReadOnDemoEnd(sr, gameflow);
        SkipBytes(sr, Constants.Unknown1Length);
        ReadNumLevels(sr, gameflow);
        ReadNumChapterScreens(sr, gameflow);
        ReadNumTitles(sr, gameflow);
        ReadNumFmvs(sr, gameflow);
        ReadNumCutscenes(sr, gameflow);
        ReadNumDemoLevels(sr, gameflow);
        ReadTitleSoundId(sr, gameflow);
        ReadSingleLevel(sr, gameflow);
        SkipBytes(sr, Constants.Unknown2Length);
        ReadFlags(sr, gameflow);
        SkipBytes(sr, Constants.Unknown3Length);
        ReadXorKey(sr, gameflow);
        ReadLanguageId(sr, gameflow);
        ReadSecretSoundId(sr, gameflow);
        SkipBytes(sr, Constants.Unknown4Length);
        gameflow.LevelStrings = ReadTpcStringArray(gameflow.NumLevels, sr);
        gameflow.ChapterScreenStrings = ReadTpcStringArray(gameflow.NumChapterScreens, sr);
        gameflow.TitleStrings = ReadTpcStringArray(gameflow.NumTitles, sr);
        gameflow.FmvStrings = ReadTpcStringArray(gameflow.NumFmvs, sr);
        gameflow.LevelPathStrings = ReadTpcStringArray(gameflow.NumLevels, sr);
        gameflow.CutscenePathStrings = ReadTpcStringArray(gameflow.NumCutscenes, sr);
        return gameflow;
    }

    private void ReadVersion(BinaryReader reader, Gameflow gameflow)
    {
        var version = reader.ReadUInt32();
        gameflow.Version = version;
    }

    private void ReadDescription(BinaryReader reader, Gameflow gameflow)
    {
        var rawBytes = reader.ReadBytes(Constants.DescriptionLength);
        if (rawBytes.Length != Constants.DescriptionLength)
        {
            _logger.LogDebug("Expected to read {DescriptionLength}, but actually read {ActualCount}", Constants.DescriptionLength, rawBytes.Length);
        }

        gameflow.Description = rawBytes.GetNullTerminatedString();
    }

    private void ReadGameflowSize(BinaryReader reader, Gameflow gameflow)
    {
        var size = reader.ReadUInt16();
        if (size != Constants.ExpectedGameflowSize)
        {
            _logger.LogDebug("Expected to gameflow to be {ExpectedGameflowSize}, but read value was {ActualSize}", Constants.ExpectedGameflowSize, size);
        }
        gameflow.GameflowSize = size;
    }

    private void ReadFirstOption(BinaryReader reader, Gameflow gameflow)
    {
        var firstOption = reader.ReadInt32();
        gameflow.FirstOption = firstOption;
    }

    private void ReadTitleReplace(BinaryReader reader, Gameflow gameflow)
    {
        var titleReplace = reader.ReadInt32();
        gameflow.TitleReplace = titleReplace;
    }

    private void ReadOnDeathDemoMode(BinaryReader reader, Gameflow gameflow)
    {
        var onDeathDemoMode = reader.ReadInt32();
        gameflow.OnDeathDemoMode = onDeathDemoMode;
    }

    private void ReadOnDeathInGame(BinaryReader reader, Gameflow gameflow)
    {
        var onDeathInGame = reader.ReadInt32();
        gameflow.OnDeathInGame = onDeathInGame;
    }

    private void ReadDemoTime(BinaryReader reader, Gameflow gameflow)
    {
        var demoTime = reader.ReadUInt32();
        gameflow.DemoTime = demoTime;
    }

    private void ReadOnDemoInterrupt(BinaryReader reader, Gameflow gameflow)
    {
        var onDemoInterrupt = reader.ReadInt32();
        gameflow.OnDemoInterrupt = onDemoInterrupt;
    }

    private void ReadOnDemoEnd(BinaryReader reader, Gameflow gameflow)
    {
        var onDemoEnd = reader.ReadInt32();
        gameflow.OnDemoEnd = onDemoEnd;
    }

    private void SkipBytes(BinaryReader reader, int toSkip)
    {
        reader.ReadBytes(toSkip);
    }

    private void ReadNumLevels(BinaryReader reader, Gameflow gameflow)
    {
        var numLevels = reader.ReadUInt16();
        gameflow.NumLevels = numLevels;
    }

    private void ReadNumChapterScreens(BinaryReader reader, Gameflow gameflow)
    {
        var numChapterScreens = reader.ReadUInt16();
        gameflow.NumChapterScreens = numChapterScreens;
    }

    private void ReadNumTitles(BinaryReader reader, Gameflow gameflow)
    {
        var numTitles = reader.ReadUInt16();
        gameflow.NumTitles = numTitles;
    }

    private void ReadNumFmvs(BinaryReader reader, Gameflow gameflow)
    {
        var numFmvs = reader.ReadUInt16();
        gameflow.NumFmvs = numFmvs;
    }

    private void ReadNumCutscenes(BinaryReader reader, Gameflow gameflow)
    {
        var numCutscenes = reader.ReadUInt16();
        gameflow.NumCutscenes = numCutscenes;
    }
    
    private void ReadNumDemoLevels(BinaryReader reader, Gameflow gameflow)
    {
        var numDemoLevels = reader.ReadUInt16();
        gameflow.NumDemoLevels = numDemoLevels;
    }

    private void ReadTitleSoundId(BinaryReader reader, Gameflow gameflow)
    {
        var titleSoundId = reader.ReadUInt16();
        gameflow.TitleSoundId = titleSoundId;
    }

    private void ReadSingleLevel(BinaryReader reader, Gameflow gameflow)
    {
        var singleLevel = reader.ReadUInt16();
        gameflow.SingleLevel = singleLevel;
    }

    private void ReadFlags(BinaryReader reader, Gameflow gameflow)
    {
        var flags = reader.ReadUInt16();
        gameflow.Flags = (GameflowFlags)flags;
    }

    private void ReadXorKey(BinaryReader reader, Gameflow gameflow)
    {
        var xorKey = reader.ReadByte();
        gameflow.XorKey = xorKey;
    }

    private void ReadLanguageId(BinaryReader reader, Gameflow gameflow)
    {
        var languageId = reader.ReadByte();
        gameflow.LanguageId = (GameflowLanguage)languageId;
    }

    private void ReadSecretSoundId(BinaryReader reader, Gameflow gameflow)
    {
        var secretSoundId = reader.ReadUInt16();
        gameflow.SecretSoundId = secretSoundId;
    }

    private TpcStringArray ReadTpcStringArray(int length, BinaryReader reader)
    {

        var stringArray = new TpcStringArray(length);

        for (var i = 0; i < length; i++)
        {
            var offset = reader.ReadUInt16();
            stringArray.Offsets[i] = offset;
        }

        stringArray.TotalSize = reader.ReadUInt16();
        for (var i = 0; i < stringArray.TotalSize; i++)
        {
            var currentByte = reader.ReadByte();

            stringArray.Data![i] = currentByte;
        }
        return stringArray;
    }
}