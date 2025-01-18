using TombLauncher.Contracts.Patchers;

namespace TombLauncher.Patchers.Widescreen;

public class WidescreenPatcherParameters : IPatchParameters
{
    public string TargetFolder { get; set; }
    public bool UpdateAspectRatio { get; set; }
    public bool UpdateCameraDistance { get; set; }
    public bool UpdateFov { get; set; }
    public float OriginalAspectRatio { get; set; }
    public float TargetAspectRatio { get; set; }
    public short TargetCameraDistance { get; set; }
    public short OriginalCameraDistance { get; set; }
    public short HorizontalResolution { get; set; }
}