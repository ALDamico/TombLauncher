namespace TombLauncher.Configuration.Sections;

public interface ICompatibilityConfig
{
    string? WinePath { get; }
    string? WinePrefix { get; }
}