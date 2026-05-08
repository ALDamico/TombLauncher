using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.SupportMatrix;

namespace TombLauncher.Core.PlatformSpecific.SupportMatrix;

public class WindowsSupportMatrix : ISupportMatrix
{
    public WindowsSupportMatrix()
    {
        Matrix = new Dictionary<GameEngine, EngineSupportState>()
        {
            { GameEngine.Unknown, EngineSupportState.NoSupport },
            { GameEngine.TombRaider1, EngineSupportState.FullSupport },
            { GameEngine.TombRaider1Dos, EngineSupportState.NoSupport },
            { GameEngine.TombRaider2, EngineSupportState.FullSupport },
            { GameEngine.TombRaider3, EngineSupportState.FullSupport },
            { GameEngine.TombRaider4, EngineSupportState.FullSupport },
            { GameEngine.TombRaider5, EngineSupportState.FullSupport },
            { GameEngine.Ten, EngineSupportState.FullSupport },
            { GameEngine.Tr1x, EngineSupportState.FullSupport },
            { GameEngine.Tr2x, EngineSupportState.FullSupport },
            { GameEngine.TombAti, EngineSupportState.FullSupport },
            { GameEngine.Tomb2Main, EngineSupportState.FullSupport },
            { GameEngine.Tomb3CommunityEdition, EngineSupportState.FullSupport }
        };
    }
    public IReadOnlyDictionary<GameEngine, EngineSupportState> Matrix { get; }
}