using TombLauncher.Contracts.Enums;

namespace TombLauncher.Contracts.EngineDetectors;

public class EngineDetectorResult
{
    public string? InstallFolder { get; set; } = null!;
    public string? ExecutablePath { get; set; } = null!;
    public string? UniversalLauncherPath { get; set; } = null!;
    public GameEngine GameEngine { get; set; }
    public string? SetupExecutablePath { get; set; } = null!;
    public string? SetupArgs { get; set; } = null!;
    public string? CommunitySetupExecutablePath { get; set; } = null!;
}