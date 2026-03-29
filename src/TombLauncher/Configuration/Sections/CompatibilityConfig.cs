using TombLauncher.Contracts.Enums;

namespace TombLauncher.Configuration.Sections;

public class CompatibilityConfig : ICompatibilityConfig
{
    public CompatibilityTool CompatibilityTool { get; set; } = CompatibilityTool.Wine;
    public string? WinePath { get; set; }
    public string? WinePrefix { get; set; }
    public string? ProtonPath { get; set; }
}