using System;
using NetSparkleUpdater.Interfaces;

namespace TombLauncher.Updater;

public class ConsoleLogWriter : ILogger
{
    public void PrintMessage(string message, params object[] arguments)
    {
        Console.WriteLine(message, arguments);
    }
}