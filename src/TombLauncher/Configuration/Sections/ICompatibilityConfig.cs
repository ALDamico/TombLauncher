using TombLauncher.Contracts.Enums;

namespace TombLauncher.Configuration.Sections;

public interface ICompatibilityConfig
{
    CompatibilityTool CompatibilityTool { get; }
    string? WinePath { get; }
    string? CompatibilityPrefixPath { get; }
    string? ProtonPath { get; }
}