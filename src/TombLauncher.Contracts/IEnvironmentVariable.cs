namespace TombLauncher.Contracts;

public interface IEnvironmentVariable
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string VariableName { get; set; }
    public string VariableValue { get; set; }
}