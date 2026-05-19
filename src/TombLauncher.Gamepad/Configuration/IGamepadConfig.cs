namespace TombLauncher.Gamepad.Configuration;

public interface IGamepadConfig
{
    GamepadTool? GamepadTool { get; }
    string? ToolPath { get; }
    Dictionary<string, string?>? Profiles { get; }
}