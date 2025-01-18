using System.ComponentModel;

namespace TombLauncher.Contracts.Enums;

public enum SaveGameBackupMode
{
    [Description("None")]
    None,
    [Description("Start of level")]
    StartOfLevel,
    [Description("All")]
    All
}