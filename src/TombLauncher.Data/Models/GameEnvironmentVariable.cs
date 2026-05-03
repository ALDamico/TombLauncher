namespace TombLauncher.Data.Models;

public class GameEnvironmentVariable
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;
    public string VariableName { get; set; } = string.Empty;
    public string VariableValue { get; set; } = string.Empty;
}