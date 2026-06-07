using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TombLauncher.Gamepad.Services;

public class AntiMicroXGamepadService : IGamepadService
{
    private readonly ILogger<AntiMicroXGamepadService> _logger;

    public AntiMicroXGamepadService(ILogger<AntiMicroXGamepadService> logger)
    {
        _logger = logger;
    }
    public bool WasAlreadyRunning { get; private set; }

    private Process? _ownedProcess;
    public void PrepareForGame(string executablePath, string profilePath)
    {
        WasAlreadyRunning = false;
        var runningInstance = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executablePath));
            
        if (runningInstance.Length != 0)
        {
            WasAlreadyRunning = true;
        }

        try
        {
            _ownedProcess = Process.Start(new ProcessStartInfo(executablePath)
                { ArgumentList = { "--hidden", "--profile", profilePath } });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Exception when loading AntiMicroX profile");
        }
    }

    public Task TeardownAsync(string executablePath, string profilePath)
    {
        if (WasAlreadyRunning)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo(executablePath)
                {
                    ArgumentList = { "--hidden", "--unload", profilePath }
                };
                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception when unloading AntiMicroX profile");
            }
        }
        else
        {
            try
            {
                _ownedProcess?.Kill();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Owned process had already exited");
            }
        }

        return Task.CompletedTask;
    }
}