using TombLauncher.Contracts.Enums;

namespace TombLauncher.ViewModels;

public class CameraDistanceOptionViewModel
{
    public CameraDistancePreset Value { get; init; }
    public string DisplayText { get; init; } = "";
    public bool IsCustom => Value == CameraDistancePreset.Custom;
}
