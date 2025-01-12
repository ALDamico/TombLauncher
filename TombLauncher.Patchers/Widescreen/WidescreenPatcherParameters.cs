using TombLauncher.Contracts.Patchers;

namespace TombLauncher.Patchers.Widescreen;

public class WidescreenPatcherParameters : IPatchParameters
{
    public string TargetFolder { get; set; }
    public bool UpdateAspectRatio { get; set; }
    public bool UpdateCameraDistance { get; set; }
    public float OriginalAspectRatio { get; set; }
    public float TargetAspectRatio { get; set; }
    public float TargetCameraDistance { get; set; }
    public float OriginalCameraDistance { get; set; }
}