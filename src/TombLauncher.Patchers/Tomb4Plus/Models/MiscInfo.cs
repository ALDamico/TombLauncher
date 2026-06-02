namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class MiscInfo
{
    public byte? TextOrCriticalBarBlinkInterval { get; set; }
    public byte? LegendTimer { get; set; }
    public bool? LaraImpalesOnSpikes { get; set; }
    public bool? DartsPoisonFix { get; set; }
    public bool? AlwaysExitFromStatisticsScreen { get; set; }
    public bool? DisableMotorbikeHeadlights { get; set; }
    public bool? DrawLegendOnFlyby { get; set; }
    public bool? EnableRicochetSoundEffect { get; set; }
    public bool? TrepSwitchMaker { get; set; }
    public short? TrepSwitchOnOcb1Anim { get; set; }
    public short? TrepSwitchOffOcb1Anim { get; set; }
    public short? TrepSwitchOnOcb2Anim { get; set; }
    public short? TrepSwitchOffOcb2Anim { get; set; }
    public short? TrepSwitchOnOcb5Anim { get; set; }
    public short? TrepSwitchOffOcb5Anim { get; set; }
    public short? TrepSwitchOnOcb6Anim { get; set; }
    public short? TrepSwitchOffOcb6Anim { get; set; }
    public bool? EnableSmashingAndKillingRollingBalls { get; set; }
    public bool? EnableTeethSpikesKillEnemies { get; set; }
    public bool? EnableStandingPushables { get; set; }

    public bool HasAnyValue() =>
        TextOrCriticalBarBlinkInterval != null ||
        LegendTimer != null ||
        LaraImpalesOnSpikes != null ||
        DartsPoisonFix != null ||
        AlwaysExitFromStatisticsScreen != null ||
        DisableMotorbikeHeadlights != null ||
        DrawLegendOnFlyby != null ||
        EnableRicochetSoundEffect != null ||
        TrepSwitchMaker != null ||
        TrepSwitchOnOcb1Anim != null ||
        TrepSwitchOffOcb1Anim != null ||
        TrepSwitchOnOcb2Anim != null ||
        TrepSwitchOffOcb2Anim != null ||
        TrepSwitchOnOcb5Anim != null ||
        TrepSwitchOffOcb5Anim != null ||
        TrepSwitchOnOcb6Anim != null ||
        TrepSwitchOffOcb6Anim != null ||
        EnableSmashingAndKillingRollingBalls != null ||
        EnableTeethSpikesKillEnemies != null ||
        EnableStandingPushables != null;
}