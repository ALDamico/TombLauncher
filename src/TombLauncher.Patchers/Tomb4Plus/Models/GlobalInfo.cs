namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class GlobalInfo
{
    public int ManifestCompatibilityVersion { get; set; }
    public string? GameName { get; set; }
    public string? Authors { get; set; }
    public string? ReleaseDate { get; set; }
    public string? GameUserDirName { get; set; }
    public int TrngVersionMajor { get; set; }
    public int TrngVersionMinor { get; set; }
    public int TrngVersionMaintainence { get; set; }
    public int TrngVersionBuild { get; set; }
    public bool TrngVersionIsPlus { get; set; } // TODO Check if we need to replicate the OG Python bug *exactly*
    public object? FurrData { get; set; } // TODO To be defined
    public bool? TrngFlipEffectsEnabled { get; set; }
    public bool? TrngRollingBallExtendedOcb { get; set; }
    public bool? TrngStaticsExtendedOcb { get; set; }
    public bool? TrngPushableExtendedOcb { get; set; }
    public bool? TrepUsingExtendedSaves { get; set; }
    public bool? TomoEnableWeatherFlipeffect { get; set; }
    public bool? TomoSwapWhitelightForTeleporter { get; set; }
}