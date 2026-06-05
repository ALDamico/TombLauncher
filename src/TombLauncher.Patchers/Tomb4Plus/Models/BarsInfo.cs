namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class BarsInfo
{
    public BarStyle? HealthBar { get; set; }
    public BarStyle? PoisonBar { get; set; }
    public BarStyle? AirBar { get; set; }
    public BarStyle? SprintBar { get; set; }
    public BarStyle? LoadingBar { get; set; }

    public bool HasAnyValue() => HealthBar != null || PoisonBar != null || AirBar != null || SprintBar != null ||
                                 LoadingBar != null;
}