using TombLauncher.Patchers.OriginalEngines.Models;
using TombLauncher.Patchers.OriginalEngines.Parsers;

namespace TombLauncher.Tests;

public class GameflowReaderTests
{
    // https://trcustoms.org/levels/3826
    private const string InputFile = "Data/silver_machine.dat";
    private readonly Gameflow _gameflow = new GameflowReader(null!).ReadGameflow(InputFile);

    [Fact]
    public void GameflowReader_Version_ShouldEqualThree()
    {
        Assert.Equal(3U, _gameflow.Version);
    }

    [Fact]
    public void GameflowReader_Description_ShouldContainTombRaiderII()
    {
        Assert.Contains("Tomb Raider II Script File", _gameflow.Description);
    }

    [Fact]
    public void GameflowReader_GameflowSize_ShouldEqual128()
    {
        Assert.Equal(128, _gameflow.GameflowSize);
    }

    [Fact]
    public void GameflowReader_FirstOption_ShouldEqual1280()
    {
        Assert.Equal(1280, _gameflow.FirstOption);
    }

    [Fact]
    public void GameflowReader_TitleReplace_ShouldEqualMinusOne()
    {
        Assert.Equal(-1, _gameflow.TitleReplace);
    }

    [Fact]
    public void GameflowReader_OnDeathDemoMode_ShouldEqual1280()
    {
        Assert.Equal(1280, _gameflow.OnDeathDemoMode);
    }

    [Fact]
    public void GameflowReader_OnDeathInGame_ShouldEqualZero()
    {
        Assert.Equal(0, _gameflow.OnDeathInGame);
    }

    [Fact]
    public void GameflowReader_DemoTime_ShouldEqual900()
    {
        Assert.Equal(900U, _gameflow.DemoTime);
    }

    [Fact]
    public void GameflowReader_OnDemoInterrupt_ShouldEqual1280()
    {
        Assert.Equal(1280, _gameflow.OnDemoInterrupt);
    }

    [Fact]
    public void GameflowReader_OnDemoEnd_ShouldEqual1280()
    {
        Assert.Equal(1280, _gameflow.OnDemoEnd);
    }

    [Fact]
    public void GameflowReader_NumLevels_ShouldEqualSix()
    {
        Assert.Equal(6, _gameflow.NumLevels);
    }

    [Fact]
    public void GameflowReader_NumChapterScreens_ShouldEqualZero()
    {
        Assert.Equal(0, _gameflow.NumChapterScreens);
    }

    [Fact]
    public void GameflowReader_NumTitles_ShouldEqualThree()
    {
        Assert.Equal(3, _gameflow.NumTitles);
    }

    [Fact]
    public void GameflowReader_NumFmvs_ShouldEqualZero()
    {
        Assert.Equal(0, _gameflow.NumFmvs);
    }

    [Fact]
    public void GameflowReader_NumCutscenes_ShouldEqualZero()
    {
        Assert.Equal(0, _gameflow.NumCutscenes);
    }

    [Fact]
    public void GameflowReader_NumDemoLevels_ShouldEqualZero()
    {
        Assert.Equal(0, _gameflow.NumDemoLevels);
    }

    [Fact]
    public void GameflowReader_TitleSoundId_ShouldEqual64()
    {
        Assert.Equal(64, _gameflow.TitleSoundId);
    }

    [Fact]
    public void GameflowReader_SingleLevel_ShouldBeDisabled()
    {
        Assert.Equal(ushort.MaxValue, _gameflow.SingleLevel);
    }

    [Fact]
    public void GameflowReader_Flags_ShouldContainUseXor()
    {
        Assert.True(_gameflow.Flags.HasFlag(GameflowFlags.UseXor));
    }

    [Fact]
    public void GameflowReader_Flags_ShouldContainEnableCheatCode()
    {
        Assert.True(_gameflow.Flags.HasFlag(GameflowFlags.EnableCheatCode));
    }

    [Fact]
    public void GameflowReader_XorKey_ShouldEqual166()
    {
        Assert.Equal(166, _gameflow.XorKey);
    }

    [Fact]
    public void GameflowReader_LanguageId_ShouldBeEnglish()
    {
        Assert.Equal(GameflowLanguage.English, _gameflow.LanguageId);
    }

    [Fact]
    public void GameflowReader_SecretSoundId_ShouldEqual47()
    {
        Assert.Equal(47, _gameflow.SecretSoundId);
    }

    // String arrays — structural

    [Fact]
    public void GameflowReader_LevelStrings_ShouldHaveNumLevelsEntries()
    {
        Assert.NotNull(_gameflow.LevelStrings);
        Assert.Equal(_gameflow.NumLevels, _gameflow.LevelStrings.Count);
    }

    [Fact]
    public void GameflowReader_ChapterScreenStrings_ShouldBeEmpty()
    {
        Assert.NotNull(_gameflow.ChapterScreenStrings);
        Assert.Equal(0, _gameflow.ChapterScreenStrings.Count);
    }

    [Fact]
    public void GameflowReader_TitleStrings_ShouldHaveNumTitlesEntries()
    {
        Assert.NotNull(_gameflow.TitleStrings);
        Assert.Equal(_gameflow.NumTitles, _gameflow.TitleStrings.Count);
    }

    [Fact]
    public void GameflowReader_FmvStrings_ShouldBeEmpty()
    {
        Assert.NotNull(_gameflow.FmvStrings);
        Assert.Equal(0, _gameflow.FmvStrings.Count);
    }

    [Fact]
    public void GameflowReader_LevelPathStrings_ShouldHaveNumLevelsEntries()
    {
        Assert.NotNull(_gameflow.LevelPathStrings);
        Assert.Equal(_gameflow.NumLevels, _gameflow.LevelPathStrings.Count);
    }

    [Fact]
    public void GameflowReader_CutscenePathStrings_ShouldBeEmpty()
    {
        Assert.NotNull(_gameflow.CutscenePathStrings);
        Assert.Equal(0, _gameflow.CutscenePathStrings.Count);
    }

    [Fact]
    public void GameflowReader_GameStrings_ShouldHave89Entries()
    {
        Assert.NotNull(_gameflow.GameStrings);
        Assert.Equal(89, _gameflow.GameStrings.Count);
    }

    [Fact]
    public void GameflowReader_PcStrings_ShouldHave41Entries()
    {
        Assert.NotNull(_gameflow.PcStrings);
        Assert.Equal(41, _gameflow.PcStrings.Count);
    }

    // Sequences

    [Fact]
    public void GameflowReader_SequenceOffsets_ShouldHaveNumLevelsPlusOneEntries()
    {
        Assert.NotNull(_gameflow.SequenceOffsets);
        Assert.Equal(_gameflow.NumLevels + 1, _gameflow.SequenceOffsets.Length);
    }

    [Fact]
    public void GameflowReader_SequenceNumBytes_ShouldEqual144()
    {
        Assert.Equal(144, _gameflow.SequenceNumBytes);
    }

    [Fact]
    public void GameflowReader_Sequences_ShouldNotBeEmpty()
    {
        Assert.NotNull(_gameflow.Sequences);
        Assert.NotEmpty(_gameflow.Sequences);
    }

    [Fact]
    public void GameflowReader_Sequences_ShouldHaveAtLeastOneEndOpcodePerLevel()
    {
        Assert.NotNull(_gameflow.Sequences);
        var endCount = _gameflow.Sequences.Count(s => s.Opcode == SequenceOpcode.End);
        Assert.Equal(_gameflow.NumLevels + 1, endCount);
    }

    [Fact]
    public void GameflowReader_Sequences_EndOpcodeShouldHaveNoArgument()
    {
        Assert.NotNull(_gameflow.Sequences);
        var endOpcodes = _gameflow.Sequences.Where(s => s.Opcode == SequenceOpcode.End);
        Assert.All(endOpcodes, s => Assert.Null(s.Argument));
    }

    // Demo levels

    [Fact]
    public void GameflowReader_DemoLevelIds_ShouldBeEmpty()
    {
        Assert.NotNull(_gameflow.DemoLevelIds);
        Assert.Empty(_gameflow.DemoLevelIds);
    }

    // Puzzle / Pickup / Key strings — structural

    [Fact]
    public void GameflowReader_PuzzleStrings_ShouldHaveFourSlots()
    {
        Assert.Equal(4, _gameflow.PuzzleStrings.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void GameflowReader_PuzzleStrings_EachSlotShouldHaveNumLevelsEntries(int slot)
    {
        Assert.Equal(_gameflow.NumLevels, _gameflow.PuzzleStrings[slot].Count);
    }

    [Fact]
    public void GameflowReader_PickupStrings_ShouldHaveTwoSlots()
    {
        Assert.Equal(2, _gameflow.PickupStrings.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void GameflowReader_PickupStrings_EachSlotShouldHaveNumLevelsEntries(int slot)
    {
        Assert.Equal(_gameflow.NumLevels, _gameflow.PickupStrings[slot].Count);
    }

    [Fact]
    public void GameflowReader_KeyStrings_ShouldHaveFourSlots()
    {
        Assert.Equal(4, _gameflow.KeyStrings.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void GameflowReader_KeyStrings_EachSlotShouldHaveNumLevelsEntries(int slot)
    {
        Assert.Equal(_gameflow.NumLevels, _gameflow.KeyStrings[slot].Count);
    }
}
