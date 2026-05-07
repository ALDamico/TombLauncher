using TombLauncher.Contracts.EngineDetectors;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Patchers.Widescreen;

public class WidescreenPatcherService
{
    public WidescreenPatcherService(WidescreenPatcher patcher, IEngineDetector engineDetector)
    {
        _patcher = patcher;
        _engineDetector = engineDetector;
    }

    private readonly WidescreenPatcher _patcher;
    private readonly IEngineDetector _engineDetector;

    public bool Check60FpsSupport(string gameFolder)
    {
        var detectionResult = _engineDetector.Detect(gameFolder);
        return detectionResult.GameEngine is GameEngine.TombRaider2 or GameEngine.TombRaider3;
    }
}