using System.Collections.Generic;
using TombLauncher.Gamepad;
using TombLauncher.Gamepad.Configuration;

namespace TombLauncher.Configuration.Sections;

public class GamepadConfig : IGamepadConfig
{
    public GamepadTool? GamepadTool { get; set; }
    public string? ToolPath { get; set; }
    public Dictionary<string, string?>? Profiles { get; set; }
}