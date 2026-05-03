namespace TombLauncher.Patchers.Shared.Models;

public class LevelInfo
{
    public required string Title { get; set; }
    public required string FilePath { get; set; }
    public List<SequenceInfo> Sequence { get; set; } = [];
    public List<string> PuzzleItemNames { get; set; } = [];
    public List<string> PickupItemNames { get; set; } = [];
    public List<string> KeyNames { get; set; } = [];
}