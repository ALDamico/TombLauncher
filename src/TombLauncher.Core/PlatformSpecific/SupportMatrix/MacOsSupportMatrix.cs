using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.SupportMatrix;

namespace TombLauncher.Core.PlatformSpecific.SupportMatrix;

public class MacOsSupportMatrix : ISupportMatrix
{
    public MacOsSupportMatrix()
    {
        Matrix = new Dictionary<GameEngine, EngineSupportState>()
        {
            { GameEngine.Unknown, EngineSupportState.NoSupport },
            { GameEngine.TombRaider1, EngineSupportState.NoSupport },
            { GameEngine.TombRaider1Dos, EngineSupportState.NoSupport },
            { GameEngine.TombRaider2, EngineSupportState.NoSupport },
            { GameEngine.TombRaider3, EngineSupportState.NoSupport },
            { GameEngine.TombRaider4, EngineSupportState.NoSupport },
            { GameEngine.TombRaider5, EngineSupportState.NoSupport },
            { GameEngine.Ten, EngineSupportState.NoSupport },
            { GameEngine.Tr1x, EngineSupportState.NoSupport },
            { GameEngine.Tr2x, EngineSupportState.NoSupport },
            { GameEngine.TombAti, EngineSupportState.NoSupport },
            { GameEngine.Tomb2Main, EngineSupportState.NoSupport },
            { GameEngine.Tomb3CommunityEdition, EngineSupportState.NoSupport },
            { GameEngine.Trx, EngineSupportState.NoSupport }
        };
    }
    public IReadOnlyDictionary<GameEngine, EngineSupportState> Matrix { get; }
}