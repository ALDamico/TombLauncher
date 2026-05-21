using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.PlatformSpecific.Models;
using TombLauncher.Contracts.PlatformSpecific.SupportMatrix;

namespace TombLauncher.Core.PlatformSpecific.SupportMatrix;

public class MacOsSupportMatrix : ISupportMatrix
{
    public MacOsSupportMatrix()
    {
        Matrix = new Dictionary<GameEngine, GameSupportMatrixEntry>()
        {
            {
                GameEngine.Unknown,
                new()
                {
                    Engine = GameEngine.Unknown,
                    SupportState = EngineSupportState.NoSupport,
                    ShortDescription = "CANT_DETERMINE_SUPPORT_FOR_UNKNOWN_ENGINE"                }
            },
            {
                GameEngine.TombRaider1,
                new()
                {
                    Engine = GameEngine.TombRaider1,
                    SupportState = EngineSupportState.NoSupport,
                    ShortDescription = "TOMB_RAIDER_1_MACOS_SUPPORT_SHORT_DESCRIPTION"                }
            },
            {
                GameEngine.TombRaider1Dos,
                new()
                {
                    Engine = GameEngine.TombRaider1Dos,
                    SupportState = EngineSupportState.NoSupport,
                    ShortDescription = "TOMB_RAIDER_1_DOS_MACOS_SUPPORT_SHORT_DESCRIPTION"                }
            },
            {
                GameEngine.TombRaider2,
                new()
                {
                    Engine = GameEngine.TombRaider2,
                    SupportState = EngineSupportState.NoSupport,
                    ShortDescription = "TOMB_RAIDER_2_MACOS_SUPPORT_SHORT_DESCRIPTION"                }
            },
            {
                GameEngine.TombRaider3,
                new()
                {
                    Engine = GameEngine.TombRaider3,
                    SupportState = EngineSupportState.NoSupport,
                    ShortDescription = "TOMB_RAIDER_3_MACOS_SUPPORT_SHORT_DESCRIPTION"                }
            },
            {
                GameEngine.TombRaider4,
                new()
                {
                    Engine = GameEngine.TombRaider4,
                    SupportState = EngineSupportState.NoSupport,
                    ShortDescription = "TOMB_RAIDER_4_MACOS_SUPPORT_SHORT_DESCRIPTION"                }
            },
            {
                GameEngine.TombRaider5,
                new()
                {
                    Engine = GameEngine.TombRaider5,
                    SupportState = EngineSupportState.NoSupport,
                    ShortDescription = "TOMB_RAIDER_5_MACOS_SUPPORT_SHORT_DESCRIPTION"                }
            },
            {
                GameEngine.Ten,
                new()
                {
                    Engine = GameEngine.Ten,
                    SupportState = EngineSupportState.NoSupport,
                    ShortDescription = "TEN_MACOS_SUPPORT_SHORT_DESCRIPTION"                }
            },
            {
                GameEngine.Tr1x,
                new()
                {
                    Engine = GameEngine.Tr1x,
                    SupportState = EngineSupportState.NoSupport,
                    ShortDescription = "TR1X_MACOS_SUPPORT_SHORT_DESCRIPTION"                }
            },
            {
                GameEngine.Tr2x,
                new()
                {
                    Engine = GameEngine.Tr2x,
                    SupportState = EngineSupportState.NoSupport,
                    ShortDescription = "TR2X_MACOS_SUPPORT_SHORT_DESCRIPTION"                }
            },
            {
                GameEngine.TombAti,
                new()
                {
                    Engine = GameEngine.TombAti,
                    SupportState = EngineSupportState.NoSupport,
                    ShortDescription = "TOMB_ATI_MACOS_SUPPORT_SHORT_DESCRIPTION"                }
            },
            {
                GameEngine.Tomb2Main,
                new()
                {
                    Engine = GameEngine.Tomb2Main,
                    SupportState = EngineSupportState.NoSupport,
                    ShortDescription = "TOMB_2_MAIN_MACOS_SUPPORT_SHORT_DESCRIPTION"                }
            },
            {
                GameEngine.Tomb3CommunityEdition,
                new()
                {
                    Engine = GameEngine.Tomb3CommunityEdition,
                    SupportState = EngineSupportState.NoSupport,
                    ShortDescription = "TOMB_3_COMMUNITY_MACOS_SUPPORT_SHORT_DESCRIPTION"                }
            },
            {
                GameEngine.Trx,
                new()
                {
                    Engine = GameEngine.Trx,
                    SupportState = EngineSupportState.NoSupport,
                    ShortDescription = "TRX_MACOS_SUPPORT_SHORT_DESCRIPTION"                }
            },
        };
    }

    public IReadOnlyDictionary<GameEngine, GameSupportMatrixEntry> Matrix { get; }
}
