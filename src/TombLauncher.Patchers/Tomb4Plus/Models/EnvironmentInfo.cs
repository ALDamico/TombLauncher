namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class EnvironmentInfo
{
    public bool? DisableDistanceLimit { get; set; }
    public int? FarView { get; set; }
    public int? FogEndRange { get; set; }
    public int? FogStartRange { get; set; }
    public int? RoomColdFlag { get; set; }

    public bool HasAnyValue() => DisableDistanceLimit != null || FarView != null || FogEndRange != null ||
                                 FogStartRange != null || RoomColdFlag != null;
}