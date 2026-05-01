using TombLauncher.Patchers.Gameflows;

namespace TombLauncher.Tests;

public class GameflowReaderTests
{
    private const string InputFile = "Data/tombpc.dat";
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
}
