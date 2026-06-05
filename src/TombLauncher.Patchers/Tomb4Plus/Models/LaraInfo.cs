namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class LaraInfo
{
    public short? CrawlspaceJumpAnimation { get; set; }
    public short? CrawlspaceJumpPitDeepnessThreshold { get; set; }
    public short? LedgeToJumpState { get; set; }
    public short? LedgeToDownState { get; set; }

    public bool HasAnyValue() => CrawlspaceJumpAnimation != null || CrawlspaceJumpPitDeepnessThreshold != null ||
                                 LedgeToJumpState != null || LedgeToDownState != null;
}