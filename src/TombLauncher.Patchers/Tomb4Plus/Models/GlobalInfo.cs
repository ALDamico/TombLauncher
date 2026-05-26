namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class GlobalInfo
{
    public int ManifestCompatibilityVersion { get; set; }
    public string? GameName { get; set; }
    public string? Authors { get; set; }
    public string? ReleaseDate { get; set; }
    public string? GameUserDirName { get; set; }
    public int TrEngineVersion { get; set; }
    public bool TrLevelEditor { get; set; }
    public bool TrTimeExclusive { get; set; }
    public bool TrUseAdpcmAudio { get; set; }
}