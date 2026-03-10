namespace TombLauncher.Configuration.Sections;

public class SavegamesConfig : ISavegamesConfig
{
    public bool? BackupSavegamesEnabled { get; set; }
    public int? NumberOfVersionsToKeep { get; set; }
    public int SavegameProcessingDelay { get; set; }
}
