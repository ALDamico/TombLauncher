namespace TombLauncher.Contracts.Patchers;

public class PatchResult
{
    public List<FileChange> AffectedFiles { get; set; }
    public bool IsSuccessful { get; set; }
    public string Message { get; set; }

    public static PatchResult UnsuccessfulResult(string message) =>
        new PatchResult() { IsSuccessful = false, Message = message };
}
