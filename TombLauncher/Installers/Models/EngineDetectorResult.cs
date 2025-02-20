using TombLauncher.Contracts.Enums;

namespace TombLauncher.Installers.Models;

public class EngineDetectorResult
{
    public string InstallFolder { get; set; }
    public string ExecutablePath { get; set; }
    public string UniversalLauncherPath { get; set; }
    public GameEngine GameEngine { get; set; }
    public string SetupExecutablePath { get; set; }
    public string SetupArgs { get; set; }
    public string CommunitySetupExecutablePath { get; set; }
}