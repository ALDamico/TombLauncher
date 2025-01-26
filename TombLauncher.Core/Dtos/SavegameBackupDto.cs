using TombLauncher.Contracts.Enums;

namespace TombLauncher.Core.Dtos;

public class SavegameBackupDto
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public byte[] Data { get; set; }
    public DateTime BackedUpOn { get; set; }
    public FileType FileType { get; set; }
    public int GameId { get; set; }
    public string Md5 { get; set; }
    public int SlotNumber { get; set; }
    public int SaveNumber { get; set; }
    public string LevelName { get; set; }
    public int MetadataId { get; set; }
}