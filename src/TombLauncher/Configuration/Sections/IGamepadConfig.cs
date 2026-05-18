using System.Collections.Generic;
using TombLauncher.Gamepad;

namespace TombLauncher.Configuration.Sections;

public interface IGamepadConfig
{
    GamepadTool? GamepadTool { get; }
    string? ToolPath { get; }
    Dictionary<string, string?>? Profiles { get; }
}