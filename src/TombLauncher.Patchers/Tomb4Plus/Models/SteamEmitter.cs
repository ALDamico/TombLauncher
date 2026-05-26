namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class SteamEmitter
{
    public required ColorRgb StartColor { get; set; }
    public sbyte StartTime { get; set; }
    public required ColorRgb EndColor { get; set; }
    public sbyte EndTime { get; set; }
    public sbyte BlendingMode { get; set; }
    public sbyte Lifetime { get; set; }
    public byte SizeVariationLowerByte { get; set; }
    public byte SizeVariationHigherByte { get; set; }
    public byte SizeMultiplier { get; set; }
    public byte Rotation { get; set; }
    public ushort Flags { get; set; }
    public byte SpriteId { get; set; }
    public sbyte HorizontalSpeed { get; set; }
    public sbyte HorizontalCurve { get; set; }
    public int VerticalSpeed1 { get; set; }
    public sbyte VerticalSpeed2 { get; set; }
    public sbyte SpawnInterval { get; set; }
}