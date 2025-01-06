using Serilog;
using ILogger = NetSparkleUpdater.Interfaces.ILogger;

namespace TombLauncher.Updater;

public class SerilogLogWriter : ILogger
{
    public void PrintMessage(string message, params object[] arguments)
    {
        Log.Logger.Information(message, arguments);
    }
}