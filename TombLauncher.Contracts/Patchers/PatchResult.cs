namespace TombLauncher.Contracts.Patchers;

public class PatchResult
{
    public List<FileChange> AffectedFiles { get; set; }
    public bool IsSuccessful { get; set; }
    public string Message { get; set; }
}