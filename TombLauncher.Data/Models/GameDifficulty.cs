using System.ComponentModel;

namespace TombLauncher.Data.Models;

[Flags]
public enum GameDifficulty
{
    [Description("Unknown")]
    Unknown,
    [Description("Easy")]
    Easy,
    [Description("Medium")]
    Medium,
    [Description("Challenging")]
    Challenging,
    [Description("Very challenging")]
    VeryChallenging
}