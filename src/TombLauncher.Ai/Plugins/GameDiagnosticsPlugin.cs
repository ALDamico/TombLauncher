using System.ComponentModel;
using Microsoft.SemanticKernel;
using TombLauncher.Ai.Models;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Utils;

namespace TombLauncher.Ai.Plugins;

public class GameDiagnosticsPlugin
{
    public GameDiagnosticsPlugin(TroubleshootingContext troubleshootingContext)
    {
        TroubleshootingContext = troubleshootingContext;
    }

    public TroubleshootingContext TroubleshootingContext { get; }
    private const string NoTroubleshootingContextMessage = "No troubleshooting context in this session.";

    [KernelFunction]
    [Description("Use this function to get the current version of Tomb Launcher")]
    public string GetCurrentVersion()
    {
        var version = VersionUtils.GetApplicationVersion();
        if (version == null)
            return "Unable to retrieve application version.";

        return version.ToString();
    }

    [KernelFunction]
    [Description("Use this function to determine what Operating System the application is running on.")]
    public string GetCurrentPlatform()
    {
        if (OperatingSystem.IsWindows())
            return nameof(Platform.Windows);
        if (OperatingSystem.IsLinux())
            return nameof(Platform.Linux);

        return nameof(Platform.Unknown);
    }

    [KernelFunction]
    [Description("Use this function to determine the engine of the custom Tomb Raider Level you're currently troubleshooting.")]
    public string GetGameEngine()
    {
        if (!TroubleshootingContext.IsSet)
        {
            return NoTroubleshootingContextMessage;
        }

        return TroubleshootingContext.GameEngine.GetDescription();
    }

    [KernelFunction]
    [Description("Use this function to determine the exit code for the latest play session. A non-zero exit code indicates an error, while zero indicates no error. In case of non-zero exit codes, you can use the *get_std_err*, *get_std_out*, and *get_last_crash_log* to retrieve additional crash information to help troubleshoot the problem.")]
    public string GetLastExitCode()
    {
        if (!TroubleshootingContext.IsSet)
        {
            return NoTroubleshootingContextMessage;
        }

        if (TroubleshootingContext.LastExitCode == null)
        {
            return "No exit code in the troubleshooting session.";
        }

        return $"Exit code: {TroubleshootingContext.LastExitCode}";
    }

    [KernelFunction]
    [Description("Use this function to get the standard error for the latest play session. Not all custom levels may write to standard error. If that's the case, you can use *get_std_out* and *get_last_crash_log* to search for additional information to help troubleshoot the current issue.")]
    public string GetLastStdErr()
    {
        if (!TroubleshootingContext.IsSet)
        {
            return NoTroubleshootingContextMessage;
        }

        if (TroubleshootingContext.LastStdErr.IsNullOrWhiteSpace())
        {
            return "No data in stderr";
        }

        return TroubleshootingContext.LastStdErr!;
    }

    [KernelFunction]
    [Description("Use this function to get the standard output for the latest play session. If the standard output is empty, you can use *get_last_crash_log* to retrieve data from the executable's log.")]
    public string GetLastStdOut()
    {
        if (!TroubleshootingContext.IsSet)
        {
            return NoTroubleshootingContextMessage;
        }

        if (TroubleshootingContext.LastStdOut.IsNullOrWhiteSpace())
        {
            return "No data in stdout";
        }

        return TroubleshootingContext.LastStdOut!;
    }

    [KernelFunction]
    [Description("Use this function to retrieve the last crash log.")]
    public string[] GetLastCrashLog([Description("The number of lines *from the end of the file* to retrieve. If null, retrieves the entire log content")]int? maxLines = null)
    {
        if (!TroubleshootingContext.IsSet)
            return [NoTroubleshootingContextMessage];

        if (TroubleshootingContext.LastCrashLog.IsNullOrWhiteSpace())
        {
            return ["There was nothing in the log"];
        }

        var lines = TroubleshootingContext.LastCrashLog!.Split('\n');
        if (maxLines != null)
            lines = lines.TakeLast(maxLines.GetValueOrDefault()).ToArray();

        return lines;
    }
}