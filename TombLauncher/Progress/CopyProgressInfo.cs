namespace TombLauncher.Progress;

public class CopyProgressInfo
{
    public string CurrentFileName { get; set; }
    public int TotalFiles { get; set; }
    public int CurrentFile { get; set; }
    public double? Percentage => TotalFiles == 0 ? null : CurrentFile * 100 / TotalFiles;
    public string Message { get; set; }
}