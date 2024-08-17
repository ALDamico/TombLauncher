using System;
using System.ComponentModel;

namespace TombLauncher.Models;

[Flags]
public enum GameLength
{
    [Description("Unknown")]
    Unknown = 0,
    [Description("Short")]
    Short = 1,
    [Description("Medium")]
    Medium = 2,
    [Description("Long")]
    Long = 3,
    [Description("Very long")]
    VeryLong = 4
}