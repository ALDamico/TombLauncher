using TombLauncher.Contracts.Enums;

namespace TombLauncher.Patchers.Widescreen;

public class WidescreenPatcherParameters
{
    public required string TargetFolder { get; init; }
    public bool UpdateAspectRatio { get; init; }
    public bool UpdateCameraDistance { get; init; }
    public float TargetAspectRatio { get; init; }
    public float TargetCameraDistance { get; init; }
    public bool UpdateFov { get; init; }
    public int TargetFov { get; init; }
    public bool Update60Fps { get; init; }
    public GameEngine Engine { get; init; }
}
