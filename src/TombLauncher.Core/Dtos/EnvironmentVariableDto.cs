namespace TombLauncher.Core.Dtos;

public class EnvironmentVariableDto
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string VariableName { get; set; } = string.Empty;
    public string VariableValue { get; set; } = string.Empty;
}
