using TombLauncher.Core.Extensions;
using TombLauncher.Patchers.OriginalEngines.Models;
using TombLauncher.Patchers.OriginalEngines.Parsers;
using TombLauncher.Patchers.Shared;
using TombLauncher.Patchers.Shared.Models;

namespace TombLauncher.Tests;

public class GameflowMapperTests
{
    public GameflowMapperTests()
    {
        _gameflow = new GameflowReader(null!).ReadGameflow(InputFile);
        _dto = new GameflowMapper().ToDto(_gameflow);
    }

    // https://trcustoms.org/levels/3826
    private const string InputFile = "Data/silver_machine.dat";
    private readonly Gameflow _gameflow;
    private readonly GameflowDto _dto;

    [Fact]
    public void GameflowMapper_Levels_ShouldHave6Items()
    {
        Assert.Equal(6, _dto.Levels.Count);
    }

    [Fact]
    public void GameflowMapper_AllSequences_ShouldEndWithEndOpCode()
    {
        foreach (var level in _dto.Levels)
        {
            Assert.Equal(SequenceOpcode.End, level.Sequence.Last().Opcode);
        }
    }

    [Fact]
    public void GameflowMapper_Version_ShouldEqualThree()
    {
        Assert.Equal(3U, _dto.Version);
    }

    [Fact]
    public void GameflowMapper_DemoTimeout_ShouldEqualThirtySeconds()
    {
        Assert.Equal(TimeSpan.FromSeconds(30), _dto.DemoTimeout);
    }

    [Fact]
    public void GameflowMapper_IsSingleLevel_ShouldBeFalse()
    {
        Assert.False(_dto.IsSingleLevel);
    }

    [Fact]
    public void GameflowMapper_Flags_ShouldContainUseXor()
    {
        Assert.True(_dto.Flags.HasFlag(GameflowFlags.UseXor));
    }

    [Fact]
    public void GameflowMapper_Flags_ShouldContainEnableCheatCode()
    {
        Assert.True(_dto.Flags.HasFlag(GameflowFlags.EnableCheatCode));
    }

    [Fact]
    public void GameflowMapper_XorKey_ShouldEqual166()
    {
        Assert.Equal(166, _dto.XorKey);
    }

    [Fact]
    public void GameflowMapper_LanguageId_ShouldBeEnglish()
    {
        Assert.Equal(GameflowLanguage.English, _dto.LanguageId);
    }

    [Fact]
    public void GameflowMapper_SecretSoundId_ShouldEqual47()
    {
        Assert.Equal(47, _dto.SecretSoundId);
    }

    [Fact]
    public void GameflowMapper_TitleSoundId_ShouldEqual64()
    {
        Assert.Equal(64, _dto.TitleSoundId);
    }

    // Level structure

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void GameflowMapper_EachLevel_TitleShouldMatchDecodedLevelString(int index)
    {
        var expected = Decode(_gameflow.LevelStrings![index]);
        Assert.Equal(expected, _dto.Levels[index].Title);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void GameflowMapper_EachLevel_FilePathShouldMatchDecodedLevelPathString(int index)
    {
        var expected = Decode(_gameflow.LevelPathStrings![index]);
        Assert.Equal(expected, _dto.Levels[index].FilePath);
    }

    private string Decode(byte[] raw)
    {
        var bytes = _gameflow.Flags.HasFlag(GameflowFlags.UseXor) && _gameflow.XorKey != 0
            ? raw.Select(b => (byte)(b ^ _gameflow.XorKey)).ToArray()
            : raw;
        return bytes.GetNullTerminatedString();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void GameflowMapper_EachLevel_ShouldHaveFourPuzzleItemNames(int index)
    {
        Assert.Equal(4, _dto.Levels[index].PuzzleItemNames.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void GameflowMapper_EachLevel_ShouldHaveTwoPickupItemNames(int index)
    {
        Assert.Equal(2, _dto.Levels[index].PickupItemNames.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void GameflowMapper_EachLevel_ShouldHaveFourKeyNames(int index)
    {
        Assert.Equal(4, _dto.Levels[index].KeyNames.Count);
    }
}
