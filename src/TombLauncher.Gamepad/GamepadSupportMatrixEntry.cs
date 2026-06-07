using TombLauncher.Contracts.Enums;

namespace TombLauncher.Gamepad;

public class GamepadSupportMatrixEntry
{
    public GameEngine Engine { get; set; }
    public bool HasNativeSupport { get; set; }
}