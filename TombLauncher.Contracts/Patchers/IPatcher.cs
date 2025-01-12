namespace TombLauncher.Contracts.Patchers;

public interface IPatcher
{
    Task<PatchResult> DetectChanges(string targetFolder);
    Task<PatchResult> ApplyPatch(string targetFolder, IPatchParameters parameters);
    TombRaiderEngineDetector EngineDetector { get; set; }
}