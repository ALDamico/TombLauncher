using System.ComponentModel;

namespace TombLauncher.Contracts.Enums;

[Flags]
public enum DownloaderFeatures
{
    [Description("None")]
    None = 0,
    [Description("Game engine")]
    GameEngine = 1,
    [Description("Author")]
    Author = 1 << 1,
    [Description("Level name")]
    LevelName = 1 << 2,
    [Description("Difficulty")]
    GameDifficulty = 1 << 3,
    [Description("Setting")]
    Setting = 1 << 4,
    [Description("Rating")]
    Rating = 1 << 5,
    [Description("Game length")]
    GameLength = 1 << 6,
}