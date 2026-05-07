using TombLauncher.Contracts.Enums;

namespace TombLauncher.Ai.Models;

public class TroubleshootingContext
{
    public GameEngine GameEngine { get; set; }
    public int? LastExitCode { get; set; }
    public string? LastCrashLog { get; set; }
    public string? LastStdErr { get; set; }
    public string? LastStdOut { get; set; }
    public int GameId { get; set; }
}