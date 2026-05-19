namespace TombLauncher.Gamepad.Services;

public interface IGamepadService
{
    bool WasAlreadyRunning { get; }
    void PrepareForGame(string executablePath, string profilePath);
    Task TeardownAsync(string executablePath, string profilePath);
}