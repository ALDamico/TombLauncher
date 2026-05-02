using TombLauncher.Patchers.OriginalEngines.Models;
using TombLauncher.Patchers.OriginalEngines.Parsers;
using TombLauncher.Patchers.Shared;
using TombLauncher.Patchers.Shared.Models;

namespace TombLauncher.Tests;

public class GameflowMapperTests
{
    public GameflowMapperTests()
    {
        _gameflow =  new GameflowReader(null!).ReadGameflow(InputFile);
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
}