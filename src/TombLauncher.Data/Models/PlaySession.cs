namespace TombLauncher.Data.Models;

public class PlaySession
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game? Game { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? ExitCode { get; set; }
    public string? StdOut { get; set; }
    public string? StdErr { get; set; }
    public string? CrashFileContent { get; set; }
}