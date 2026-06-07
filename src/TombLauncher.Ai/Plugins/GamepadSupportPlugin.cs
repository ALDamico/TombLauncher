using System.ComponentModel;
using Microsoft.SemanticKernel;
using TombLauncher.Ai.Models;
using TombLauncher.Gamepad.SupportMatrix;

namespace TombLauncher.Ai.Plugins;

public class GamepadSupportPlugin
{
    private readonly GamepadSupportMatrix _gamepadSupportMatrix;

    public GamepadSupportPlugin(GamepadSupportMatrix gamepadSupportMatrix, TroubleshootingContext troubleshootingContext)
    {
        _gamepadSupportMatrix = gamepadSupportMatrix;
        TroubleshootingContext = troubleshootingContext;
    }
    
    public TroubleshootingContext TroubleshootingContext { get; }

    [KernelFunction]
    [Description(
        "Use this function to determine if the game engine of the level being troubleshot natively supports gamepads or if it requires an external tool such as AntiMicroX")]
    public string HasNativeGamepadSupport()
    {
        if (!TroubleshootingContext.IsSet)
            return "Can't give an answer: current chat has no game information.";

        var isNativelySupported = _gamepadSupportMatrix.GetGamepadSupport(TroubleshootingContext.GameEngine);
        return isNativelySupported
            ? "This level natively supports gamepads."
            : "This level does not natively support gamepads.";
    }
}