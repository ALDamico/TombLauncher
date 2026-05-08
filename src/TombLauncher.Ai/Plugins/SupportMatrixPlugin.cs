using System.ComponentModel;
using Microsoft.SemanticKernel;
using TombLauncher.Ai.Models;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.SupportMatrix;

namespace TombLauncher.Ai.Plugins;

public class SupportMatrixPlugin
{
    public SupportMatrixPlugin(ISupportMatrix supportMatrix,
        TroubleshootingContext troubleshootingContext)
    {
        TroubleshootingContext = troubleshootingContext;
        _supportMatrix = supportMatrix;
    }

    public TroubleshootingContext TroubleshootingContext { get; }
    private readonly ISupportMatrix _supportMatrix;

    [KernelFunction]
    [Description("Use this function to determine if and how the game engine of the level being troubleshot is supported on the current operating system.")]
    public string IsEngineSupported()
    {
        if (!TroubleshootingContext.IsSet)
            return "Can't give an answer: current chat has no game information.";
        var isSupported = _supportMatrix.GetEngineSupportState(TroubleshootingContext.GameEngine);
        return isSupported switch
        {
            EngineSupportState.FullSupport => "This level can run natively on the current platform",
            EngineSupportState.NativePatchingAvailable =>
                "This level can run via compatibility layer, but can be patched to run natively",
            EngineSupportState.SupportedWithCompatibilityLayer =>
                "This level can run via compatibility layer, such as Wine or Proton",
            _ => "This level is not supported in the current operating system."
        };
    }
}