namespace TombLauncher.Core.Dtos;

public class PlaySessionCrashDto
{
    public int? ExitCode { get; set; }
    public string? StdOut { get; set; }
    public string? StdErr { get; set; }
    public List<CrashFileDto> CrashFiles { get; } = [];
}