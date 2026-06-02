namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class ObjectCustomization
{
    public int? HitPoints { get; set; }
    public int? Damage1 { get; set; }
    public int? Damage2 { get; set; }
    public int? Damage3 { get; set; }

    public bool HasAnyValue() => HitPoints != null || Damage1 != null || Damage2 != null || Damage3 != null;
}