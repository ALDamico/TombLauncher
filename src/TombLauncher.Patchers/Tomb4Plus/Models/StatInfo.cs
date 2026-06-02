using TombLauncher.Core.Extensions;

namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class StatInfo
{
    public int? SecretCount { get; set; }
    public List<EquipmentModifier>? EquipmentModifiers { get; set; }

    public bool HasAnyValue() => SecretCount != null && EquipmentModifiers.IsNotNullOrEmpty();
}