using System.ComponentModel;

namespace TombLauncher.Contracts.Enums;

public enum FileType
{
    [Description("Unknown")]
    Unknown,
    [Description("Savegame")]
    Savegame,
    [Description("Savegame (start of level)")]
    SavegameStartOfLevel,
    [Description("Executable")]
    GameExecutable,
    [Description]
    SetupExecutable,
    [Description]
    CommunitySetupExecutable,
    [Description("Other")]
    Other
}