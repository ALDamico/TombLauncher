namespace TombLauncher.Contracts.Patchers;

public class FileChange
{
    public ChangeType ChangeType { get; set; }
    public string Filename { get; set; }
    public long OriginalSize { get; set; }
    public long NewSize { get; set; }
    public long? Offset { get; set; }
}