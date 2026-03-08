namespace TombLauncher.Data.Models;

public class SavegameMetadata
{
    public int Id { get; set; }
    public FileBackup FileBackup { get; set; }
    public int FileBackupId { get; set; }
    public int SlotNumber { get; set; }
    public int SaveNumber { get; set; }
    public string LevelName { get; set; }
}