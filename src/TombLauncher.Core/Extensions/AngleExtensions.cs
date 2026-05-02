namespace TombLauncher.Core.Extensions;

public static class AngleExtensions
{
    private const double CompleteAngleDegrees = 360.0;
    private const double BamFullRotation = 65536.0;
    public static double FromBam(this ushort value) => value * CompleteAngleDegrees / BamFullRotation;
    public static ushort FromDegrees(this double value) => (ushort)(value * BamFullRotation / CompleteAngleDegrees);
    public static double? FromBam(this ushort? value) => value?.FromBam();
}