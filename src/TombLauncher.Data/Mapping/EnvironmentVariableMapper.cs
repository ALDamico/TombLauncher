using TombLauncher.Contracts;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Mapping;

public class EnvironmentVariableMapper
{
    public IEnvironmentVariable ToDto(GameEnvironmentVariable environmentVariable)
    {
        return new EnvironmentVariableDto()
        {
            GameId = environmentVariable.GameId,
            Id = environmentVariable.Id,
            VariableName = environmentVariable.VariableName,
            VariableValue = environmentVariable.VariableValue
        };
    }

    public List<IEnvironmentVariable> ToDtos(IEnumerable<GameEnvironmentVariable> environmentVariables) =>
        environmentVariables.Select(ToDto).ToList();

    public GameEnvironmentVariable ToEntity(IEnvironmentVariable dto) =>
        new()
        {
            GameId = dto.GameId,
            VariableName = dto.VariableName,
            VariableValue = dto.VariableValue
        };

    public List<GameEnvironmentVariable> ToEntities(IEnumerable<IEnvironmentVariable> dtos) =>
        dtos.Select(ToEntity).ToList();
}