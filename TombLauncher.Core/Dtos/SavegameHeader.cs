namespace TombLauncher.Core.Dtos;

public class SavegameHeader
{
    public virtual string LevelName { get; set; }
    public virtual string Filepath { get; set; }
    public virtual int SaveNumber { get; set; }
    public virtual int SlotNumber { get; set; }
}