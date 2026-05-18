using System.Collections.Generic;

namespace TombLauncher.Configuration.Sections;

public class GamepadConfig : IGamepadConfig
{
    public string? AntiMicroXPath { get; set; }
    public Dictionary<string, string?>? Profiles { get; set; }
}