using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Patchers;

namespace TombLauncher.Patchers.Widescreen;

public class WidescreenPatcherParameters : IPatchParameters
{
    public required string TargetFolder { get; set; }
    public bool UpdateAspectRatio { get; set; }
    public bool UpdateCameraDistance { get; set; }
    public float OriginalAspectRatio { get; set; }
    public float TargetAspectRatio { get; set; }
    public float TargetCameraDistance { get; set; }
    public float OriginalCameraDistance { get; set; }
    public bool UpdateFov { get; set; }
    public int TargetFov { get; set; }
    public bool Update60Fps { get; set; }
    public GameEngine Engine { get; set; }
}
