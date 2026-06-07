namespace TombLauncher.Contracts.Settings;

public class UnzipBackendDto
{
    public required string Name { get; set; }
    public required string Command { get; set; }
    public required string CommandLineArguments { get; set; }
}