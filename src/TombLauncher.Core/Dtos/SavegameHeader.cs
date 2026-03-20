namespace TombLauncher.Core.Dtos;

public class SavegameHeader
{
    public required string LevelName { get; set; }
    public required string Filepath { get; set; }
    public int? SaveNumber { get; set; }
    public int SlotNumber { get; set; }
}