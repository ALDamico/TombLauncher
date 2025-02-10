using System.ComponentModel;

namespace TombLauncher.Contracts.Enums;

[Flags]
public enum GameEngine
{
    [Description("Unknown")]
    Unknown = 0,
    [Description("Tomb Raider")]
    TombRaider1 = 1,
    [Description("Tomb Raider II")]
    TombRaider2 = 2,
    [Description("Tomb Raider III: Adventures of Lara Croft")]
    TombRaider3 = 3,
    [Description("Tomb Raider: The Last Revelation")]
    TombRaider4 = 4,
    [Description("Tomb Raider Chronicles")]
    TombRaider5 = 5,
    [Description("TEN")]
    Ten = 6,
    // Lost Artefacts engines
    [Description("TR1X")]
    Tr1x = 1 << 8 | TombRaider1,
    [Description("TR2X")]
    Tr2x = 2 << 8 | TombRaider2,
    [Description("Tomb Raider (DOS)")]
    TombRaider1Dos = 1 << 9 | TombRaider1,
    [Description("TombATI")]
    TombAti = 2 << 9 | TombRaider1,
    [Description("Tomb2Main")]
    Tomb2Main = 1 << 10 | TombRaider2,
    [Description("Tomb Raider III Community Edition")]
    Tomb3CommunityEdition = 1 << 11 | TombRaider3
}