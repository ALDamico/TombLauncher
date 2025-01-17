using TombLauncher.Contracts.Enums;

namespace TombLauncher.Installers.Models;

public class EngineDetectorResult
{
    public string InstallFolder { get; set; }
    public string ExecutablePath { get; set; }
    public string UniversalLauncherPath { get; set; }
    public GameEngine GameEngine { get; set; }
}