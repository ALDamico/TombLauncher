namespace TombLauncher.Gamepad.Services;

public class NullGamepadService : IGamepadService
{
    public bool WasAlreadyRunning => true;
    public void PrepareForGame(string executablePath, string profilePath)
    {
        // NO-OP
    }

    public Task TeardownAsync(string executablePath, string profilePath)
    {
        // NO-OP
        return Task.CompletedTask;
    }
}