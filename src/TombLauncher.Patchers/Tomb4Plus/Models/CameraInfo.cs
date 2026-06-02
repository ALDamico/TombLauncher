namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class CameraInfo
{
    public short? ChaseCameraVerticalOrientation { get; set; }
    public short? LookCameraDistance { get; set; }
    public short? AddOnBattleCameraTop { get; set; }
    public short? CameraSpeed { get; set; }
    public short? NormalCameraDistance { get; set; }

    public bool HasAnyValue() => ChaseCameraVerticalOrientation != null || 
                                 LookCameraDistance != null ||
                                 AddOnBattleCameraTop != null || 
                                 CameraSpeed != null || 
                                 NormalCameraDistance != null;
}