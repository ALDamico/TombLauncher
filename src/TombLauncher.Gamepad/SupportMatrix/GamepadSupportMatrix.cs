using TombLauncher.Contracts.Enums;

namespace TombLauncher.Gamepad.SupportMatrix;

public class GamepadSupportMatrix
{
    public GamepadSupportMatrix()
    {
        Matrix = new Dictionary<GameEngine, bool>()
        {
            { GameEngine.Unknown, false },
            { GameEngine.TombRaider1, false },
            { GameEngine.TombRaider1Dos, false },
            { GameEngine.TombRaider2, false },
            { GameEngine.TombRaider3, false },
            { GameEngine.TombRaider4, false },
            { GameEngine.TombRaider5, false },
            { GameEngine.Ten, true },
            { GameEngine.Tr1x, true },
            { GameEngine.Tr2x, true },
            { GameEngine.TombAti, false },
            { GameEngine.Tomb2Main, true },
            { GameEngine.Tomb3CommunityEdition, true },
            { GameEngine.Trx, true }
        };
    }
    public IReadOnlyDictionary<GameEngine, bool> Matrix { get; }
}