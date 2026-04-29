using TombLauncher.Core.Dtos;
using TombLauncher.Data.Mapping;
using TombLauncher.Data.Models;

namespace TombLauncher.Tests;

public class EnvironmentVariableMapperTests
{
    private readonly EnvironmentVariableMapper _mapper = new();

    [Fact]
    public void ToDto_MapsAllFields()
    {
        var entity = new GameEnvironmentVariable
        {
            Id = 7, GameId = 3,
            VariableName = "WINEDEBUG", VariableValue = "-all"
        };

        var dto = _mapper.ToDto(entity);

        Assert.Equal(7, dto.Id);
        Assert.Equal(3, dto.GameId);
        Assert.Equal("WINEDEBUG", dto.VariableName);
        Assert.Equal("-all", dto.VariableValue);
    }

    [Fact]
    public void ToEntity_MapsVariableNameAndValue()
    {
        var dto = new EnvironmentVariableDto
        {
            GameId = 5, VariableName = "DXVK_HUD", VariableValue = "fps"
        };

        var entity = _mapper.ToEntity(dto);

        Assert.Equal(5, entity.GameId);
        Assert.Equal("DXVK_HUD", entity.VariableName);
        Assert.Equal("fps", entity.VariableValue);
    }

    [Fact]
    public void ToDtos_MapsAllItemsInCollection()
    {
        var entities = new[]
        {
            new GameEnvironmentVariable { Id = 1, GameId = 1, VariableName = "A", VariableValue = "1" },
            new GameEnvironmentVariable { Id = 2, GameId = 1, VariableName = "B", VariableValue = "2" },
        };

        var dtos = _mapper.ToDtos(entities);

        Assert.Equal(2, dtos.Count);
        Assert.Equal("A", dtos[0].VariableName);
        Assert.Equal("B", dtos[1].VariableName);
    }

    [Fact]
    public void ToEntities_MapsAllItemsInCollection()
    {
        var dtos = new[]
        {
            new EnvironmentVariableDto { GameId = 2, VariableName = "X", VariableValue = "x" },
            new EnvironmentVariableDto { GameId = 2, VariableName = "Y", VariableValue = "y" },
        };

        var entities = _mapper.ToEntities(dtos);

        Assert.Equal(2, entities.Count);
        Assert.Equal("X", entities[0].VariableName);
        Assert.Equal("Y", entities[1].VariableName);
    }
}
