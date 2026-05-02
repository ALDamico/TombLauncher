using TombLauncher.Patchers.OriginalEngines.Models;
using TombLauncher.Patchers.Shared.Models.Items;

namespace TombLauncher.Patchers.Shared.Models;

public class SequenceInfo
{
    public SequenceOpcode Opcode { get; set; }
    public string? LevelName { get; set; }
    public int? LevelId { get; set; }
    public int? TrackId { get; set; }
    public int? CutsceneId { get; set; }
    public int? PictureId { get; set; }
    public int? SequenceId { get; set; }
    public int? InventoryItemId { get; set; }
    public bool CountsForSecrets { get; set; }
    public int? AnimationId { get; set; }
    public double? RotationAngle { get; set; }
    public int? Depth { get; set; }
    public int? FmvId { get; set; }
    public ushort? Argument { get; set; }
    public Tr2Item? Tr2Item { get; set; }
    public Tr3Item? Tr3Item { get; set; }
}