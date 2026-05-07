using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.SupportMatrix;

namespace TombLauncher.Core.PlatformSpecific.SupportMatrix;

public class LinuxSupportMatrix : ISupportMatrix
{
    public LinuxSupportMatrix()
    {
        Matrix = new Dictionary<GameEngine, EngineSupportState>()
        {
            { GameEngine.Unknown, EngineSupportState.NoSupport },
            { GameEngine.TombRaider1, EngineSupportState.SupportedWithCompatibilityLayer },
            { GameEngine.TombRaider1Dos, EngineSupportState.NoSupport },
            { GameEngine.TombRaider2, EngineSupportState.SupportedWithCompatibilityLayer },
            { GameEngine.TombRaider3, EngineSupportState.SupportedWithCompatibilityLayer },
            { GameEngine.TombRaider4, EngineSupportState.SupportedWithCompatibilityLayer },
            { GameEngine.TombRaider5, EngineSupportState.SupportedWithCompatibilityLayer },
            { GameEngine.Ten, EngineSupportState.NoSupport },
            { GameEngine.Tr1x, EngineSupportState.SupportedWithCompatibilityLayer },
            { GameEngine.Tr2x, EngineSupportState.SupportedWithCompatibilityLayer },
            { GameEngine.TombAti, EngineSupportState.SupportedWithCompatibilityLayer },
            { GameEngine.Tomb2Main, EngineSupportState.SupportedWithCompatibilityLayer },
            { GameEngine.Tomb3CommunityEdition, EngineSupportState.SupportedWithCompatibilityLayer }
        };
    }
    public IReadOnlyDictionary<GameEngine, EngineSupportState> Matrix { get; }
}