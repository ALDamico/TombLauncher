namespace TombLauncher.Contracts.EngineDetectors;

public interface IEngineDetector
{
    EngineDetectorResult Detect(string containingFolder);
}