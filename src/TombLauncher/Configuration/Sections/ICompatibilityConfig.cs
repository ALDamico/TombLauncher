namespace TombLauncher.Configuration.Sections;

public interface ICompatibilityConfig
{
    CompatibilityTool CompatibilityTool { get; }
    string? WinePath { get; }
    string? WinePrefix { get; }
    string? ProtonPath { get; }
}