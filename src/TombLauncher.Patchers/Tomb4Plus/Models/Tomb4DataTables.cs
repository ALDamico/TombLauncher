namespace TombLauncher.Patchers.Tomb4Plus.Models;

public record EnemyHealthEntry(long Address, string Name, short Default, int SlotNumber);
public record EnemyDamageEntry(long Address, string Name, int Default, int Size,
    bool InvertSign, int[] SlotNumbers, int DamageId);