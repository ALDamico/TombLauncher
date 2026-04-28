namespace TombLauncher.Configuration.Sections;

public class CompatibilityConfig : ICompatibilityConfig
{
    public string? WinePath { get; set; }
    public string? WinePrefix { get; set; }
}