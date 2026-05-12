using TombLauncher.Contracts.Enums;

namespace TombLauncher.Configuration.Sections;

public class CompatibilityConfig : ICompatibilityConfig
{
    public CompatibilityTool CompatibilityTool { get; set; } = CompatibilityTool.Automatic;
    public string? WinePath { get; set; }
    public string? CompatibilityPrefixPath { get; set; }
    public string? ProtonPath { get; set; }
}