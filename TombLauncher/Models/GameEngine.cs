using System;
using System.ComponentModel;

namespace TombLauncher.Models;

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
    Ten = 6
}