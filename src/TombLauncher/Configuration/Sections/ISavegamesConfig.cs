namespace TombLauncher.Configuration.Sections;

public interface ISavegamesConfig
{
    bool? BackupSavegamesEnabled { get; }
    int? NumberOfVersionsToKeep { get; }
    int SavegameProcessingDelay { get; }
}
