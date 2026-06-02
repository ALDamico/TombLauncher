namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class ObjectsInfo
{
    public short? DartsInterval { get; set; }
    public short? DartsSpeed { get; set; }
    public ColorRgb? DartsColor { get; set; }
    public short? FallingBlockTimer { get; set; }
    public short? FallingBlockTremble { get; set; }
    public short? BeetleDispersion { get; set; }
    public sbyte? MagicalAttackDivider { get; set; }
    public bool? DisableMutantLocustAttack { get; set; }
    public List<ObjectCustomization>? ObjectCustomization { get; set; }

    public bool HasAnyValue() =>
        DartsInterval != null ||
        DartsSpeed != null ||
        DartsColor != null ||
        FallingBlockTimer != null ||
        FallingBlockTremble != null ||
        BeetleDispersion != null ||
        MagicalAttackDivider != null ||
        DisableMutantLocustAttack != null ||
        ObjectCustomization != null;
}