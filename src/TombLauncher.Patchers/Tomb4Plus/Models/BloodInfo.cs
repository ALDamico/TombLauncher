namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class BloodInfo
{
    public byte? BloodSize { get; set; }
    public byte? BloodIntensity { get; set; }
    public byte? BloodSpeed { get; set; }
    public byte? BloodSpreadFactorX { get; set; }
    public byte? BloodSpreadFactorY { get; set; }

    public bool HasAnyValue() =>
        BloodSize != null ||
        BloodIntensity != null ||
        BloodSpeed != null ||
        BloodSpreadFactorX != null ||
        BloodSpreadFactorY != null;
}