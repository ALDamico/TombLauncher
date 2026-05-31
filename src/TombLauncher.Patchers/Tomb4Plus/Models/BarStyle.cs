namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class BarStyle
{
    public BarRect? UpperRect { get; set; }
    public BarRect? LowerRect { get; set; }
    public BarRect? BorderRect { get; set; }
    public short? Width { get; set; }
    public byte? Height { get; set; }
    public bool? IsAnimated { get; set; }
    public short? XOffset { get; set; }
    public bool? Hidden { get; set; }
    
    public bool HasAnyValue() =>
        UpperRect != null || LowerRect != null || BorderRect != null ||
        Width != null || Height != null || IsAnimated != null || 
        Hidden != null || XOffset != null;
}