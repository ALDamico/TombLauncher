namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class GlobalLevelInfo
{
    public AudioInfo? AudioInfo { get; set; }
    public BarsInfo? BarsInfo { get; set; }
    public FontInfo? FontInfo { get; set; }
    public GfxInfo? GfxInfo { get; set; }
    public ObjectsInfo? ObjectsInfo { get; set; }
    public EnvironmentInfo? EnvironmentInfo { get; set; }
    public LaraInfo? LaraInfo { get; set; }
    public StatInfo? StatInfo { get; set; }
    public CreatureInfo? CreatureInfo { get; set; }
    public CameraInfo? CameraInfo { get; set; }
    public MiscInfo? MiscInfo { get; set; }
    public WeaponInfo? WeaponInfo { get; set; }

    // WeaponInfo intentionally excluded: OG Python does not include weapon_info in global_level_info
    public bool HasAnyValue() => (AudioInfo?.HasAnyValue() ?? false) || (BarsInfo?.HasAnyValue() ?? false) ||
                                 (FontInfo?.HasAnyValue() ?? false) || (GfxInfo?.HasAnyValue() ?? false) ||
                                 (ObjectsInfo?.HasAnyValue() ?? false) || (EnvironmentInfo?.HasAnyValue() ?? false) ||
                                 (LaraInfo?.HasAnyValue() ?? false) || (StatInfo?.HasAnyValue() ?? false) ||
                                 (CreatureInfo?.HasAnyValue() ?? false) || (CameraInfo?.HasAnyValue() ?? false) ||
                                 (MiscInfo?.HasAnyValue() ?? false) || (WeaponInfo?.HasAnyValue() ?? false);
}