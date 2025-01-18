namespace TombLauncher.Contracts.Patchers;

public interface IPatcher
{
    Version Version { get; }
    public string Author { get; }
    public string Description { get; }
    Task<PatchResult> DetectChanges(string targetFolder);
    Task<PatchResult> ApplyPatch(string targetFolder, IPatchParameters parameters);
    TombRaiderEngineDetector EngineDetector { get; set; }
    IPatcherUi GetUi();
}