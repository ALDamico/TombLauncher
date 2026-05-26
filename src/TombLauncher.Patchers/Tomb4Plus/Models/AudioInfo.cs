namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class AudioInfo
{
    public bool? NewAudioSystem { get; set; }
    public bool? OldCdTriggerSystem { get; set; }
    public bool? DisableLaraHitSfx { get; set; }
    public sbyte? LaraHitSfx { get; set; }
    public bool? DisableNoAmmoSfx { get; set; }
    public sbyte? NoAmmoSfx { get; set; }
    public byte? InsideJeepTrack { get; set; }
    public byte? OutsideJeepTrack { get; set; }
    public byte? SecretTrack { get; set; }
    public byte? FirstLoopedAudioTrack { get; set; }
}