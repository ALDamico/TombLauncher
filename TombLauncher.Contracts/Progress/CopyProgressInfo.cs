namespace TombLauncher.Contracts.Progress;

public class CopyProgressInfo
{
    public string CurrentFileName { get; set; }
    public long TotalFiles { get; set; }
    public long CurrentFile { get; set; }
    public double? Percentage => TotalFiles == 0 ? null : CurrentFile * 100 / TotalFiles;
    public string Message { get; set; }
}