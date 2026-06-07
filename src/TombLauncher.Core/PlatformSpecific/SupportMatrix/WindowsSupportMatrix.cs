using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.PlatformSpecific.Models;
using TombLauncher.Contracts.PlatformSpecific.SupportMatrix;

namespace TombLauncher.Core.PlatformSpecific.SupportMatrix;

public class WindowsSupportMatrix : ISupportMatrix
{
    public WindowsSupportMatrix()
    {
        Matrix = new Dictionary<GameEngine, GameSupportMatrixEntry>()
        {
            {
                GameEngine.Unknown,
                new()
                {
                    Engine = GameEngine.Unknown,
                    SupportState = EngineSupportState.NoSupport,
                    ShortDescription = "CANT_DETERMINE_SUPPORT_FOR_UNKNOWN_ENGINE"
                }
            },
            {
                GameEngine.TombRaider1,
                new()
                {
                    Engine = GameEngine.TombRaider1,
                    SupportState = EngineSupportState.FullSupport,
                    ShortDescription = "TOMB_RAIDER_1_WINDOWS_SUPPORT_SHORT_DESCRIPTION"
                }
            },
            {
                GameEngine.TombRaider1Dos,
                new()
                {
                    Engine = GameEngine.TombRaider1Dos,
                    SupportState = EngineSupportState.NoSupport,
                    ShortDescription = "TOMB_RAIDER_1_DOS_WINDOWS_SUPPORT_SHORT_DESCRIPTION",
                    LongDescription = "TOMB_RAIDER_1_DOS_WINDOWS_SUPPORT_LONG_DESCRIPTION"
                }
            },
            {
                GameEngine.TombRaider2,
                new()
                {
                    Engine = GameEngine.TombRaider2,
                    SupportState = EngineSupportState.FullSupport,
                    ShortDescription = "TOMB_RAIDER_2_WINDOWS_SUPPORT_SHORT_DESCRIPTION"
                }
            },
            {
                GameEngine.TombRaider3,
                new()
                {
                    Engine = GameEngine.TombRaider3,
                    SupportState = EngineSupportState.FullSupport,
                    ShortDescription = "TOMB_RAIDER_3_WINDOWS_SUPPORT_SHORT_DESCRIPTION"
                }
            },
            {
                GameEngine.TombRaider4,
                new()
                {
                    Engine = GameEngine.TombRaider4,
                    SupportState = EngineSupportState.FullSupport,
                    ShortDescription = "TOMB_RAIDER_4_WINDOWS_SUPPORT_SHORT_DESCRIPTION"
                }
            },
            {
                GameEngine.TombRaider5,
                new()
                {
                    Engine = GameEngine.TombRaider5,
                    SupportState = EngineSupportState.FullSupport,
                    ShortDescription = "TOMB_RAIDER_5_WINDOWS_SUPPORT_SHORT_DESCRIPTION"
                }
            },
            {
                GameEngine.Ten,
                new()
                {
                    Engine = GameEngine.Ten,
                    SupportState = EngineSupportState.FullSupport,
                    ShortDescription = "TEN_WINDOWS_SUPPORT_SHORT_DESCRIPTION",
                    LongDescription = "TEN_WINDOWS_SUPPORT_LONG_DESCRIPTION"
                }
            },
            {
                GameEngine.Tr1x,
                new()
                {
                    Engine = GameEngine.Tr1x,
                    SupportState = EngineSupportState.FullSupport,
                    ShortDescription = "TR1X_WINDOWS_SUPPORT_SHORT_DESCRIPTION"
                }
            },
            {
                GameEngine.Tr2x,
                new()
                {
                    Engine = GameEngine.Tr2x,
                    SupportState = EngineSupportState.FullSupport,
                    ShortDescription = "TR2X_WINDOWS_SUPPORT_SHORT_DESCRIPTION"
                }
            },
            {
                GameEngine.TombAti,
                new()
                {
                    Engine = GameEngine.TombAti,
                    SupportState = EngineSupportState.FullSupport,
                    ShortDescription = "TOMB_ATI_WINDOWS_SUPPORT_SHORT_DESCRIPTION"
                }
            },
            {
                GameEngine.Tomb2Main,
                new()
                {
                    Engine = GameEngine.Tomb2Main,
                    SupportState = EngineSupportState.FullSupport,
                    ShortDescription = "TOMB_2_MAIN_WINDOWS_SUPPORT_SHORT_DESCRIPTION"
                }
            },
            {
                GameEngine.Tomb3CommunityEdition,
                new()
                {
                    Engine = GameEngine.Tomb3CommunityEdition,
                    SupportState = EngineSupportState.FullSupport,
                    ShortDescription = "TOMB_3_COMMUNITY_WINDOWS_SUPPORT_SHORT_DESCRIPTION"
                }
            },
            {
                GameEngine.Trx,
                new()
                {
                    Engine = GameEngine.Trx,
                    SupportState = EngineSupportState.FullSupport,
                    ShortDescription = "TRX_WINDOWS_SUPPORT_SHORT_DESCRIPTION"
                }
            },
        };
    }

    public IReadOnlyDictionary<GameEngine, GameSupportMatrixEntry> Matrix { get; }
}