using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Extractors;

public partial class TrlePatchExtractor
{
    private ObjectsInfo? ReadObjectsInfo(BinaryReader binaryReader, PatchBinaryType patchType)
    {
        var objectsInfo = new ObjectsInfo();
        var objectCustomization = new List<ObjectCustomization>();

        for (var i = 0; i < Constants.ObjectCount; i++)
        {
            objectCustomization.Add(new ObjectCustomization());
        }
        
        if (patchType != PatchBinaryType.TrepExe)
            return null;

        short defaultHealth = -1;
        foreach (var row in Tomb4DataTables.EnemyHealthTable)
        {
            binaryReader.Seek(row.Address);
            var enemyName = row.Name;
            var enemyHealth = binaryReader.ReadInt16();
            defaultHealth = row.Default;
            if (enemyHealth != defaultHealth)
            {
                _logger.LogInformation("Enemy {EnemyName} has modified health: {EnemyHealth}", enemyName, enemyHealth);
                objectCustomization[row.SlotNumber].HitPoints = enemyHealth;
            }
        }

        if (binaryReader.CompareDataAtAddress(0x0005BF66, [0xE9, 0x8D, 0x5A, 0x05, 0x00, 0x90, 0x90]))
        {
            var smallScorpionHealth = binaryReader.ReadShortAt(0x000B19FF);
            var smallScorpionName = "small_scorpion";
            if (smallScorpionHealth != 8)
            {
                _logger.LogInformation("Enemy {EnemyName} has modified health: {EnemyHealth}", smallScorpionName, smallScorpionHealth);
                objectCustomization[106].HitPoints = defaultHealth; // This is likely a bug with the original Python code. As it is written, this ALWAYS assigns the scorpion the same hp as Von Croy
            }
        }

        objectsInfo.DartsInterval = binaryReader.ReadShortAt(0x00013E69).NullIf(EsseConstants.DartsIntervalDefault);
        objectsInfo.DartsSpeed = binaryReader.ReadShortAt(0x00013F3E).NullIf(EsseConstants.DartsIntervalDefault);
        objectsInfo.DartsColor = binaryReader.GetBgrColorAtAddress(0x0008B121).NullIf(EsseConstants.DartsDefaultColor);

        objectsInfo.FallingBlockTimer = binaryReader.ReadShortAt(0x00013A9F).NullIf(EsseConstants.FallingBlockTimerDefault);

        var fallingBlockTremble1 = binaryReader.ReadShortAt(0x00013AEF);
        var fallingBlockTremble2 = binaryReader.ReadShortAt(0x00013B02);
        
        if (fallingBlockTremble1 != fallingBlockTremble2)
            _logger.LogWarning("Falling block tremble mismatch! {Val1} vs {Val2}", fallingBlockTremble1, fallingBlockTremble2);

        objectsInfo.FallingBlockTremble = fallingBlockTremble1.NullIf(EsseConstants.FallingBlockTrembleDefault);

        foreach (var row in Tomb4DataTables.EnemyDamageTable)
        {
            binaryReader.Seek(row.Address);
            var damageName = row.Name;
            var damageValue = 0;

            if (row.Size == 1)
                damageValue = binaryReader.ReadSByte();
            else if (row.Size == 2)
                damageValue = binaryReader.ReadInt16();

            damageValue = damageValue * (row.InvertSign ? -1 : 1);

            var defaultDamage = row.Default;
            if (damageValue != defaultDamage)
            {
                _logger.LogInformation("Damage {DamageName} has modified value: {DamageValue}", damageName, damageValue);
                foreach (var slotNumber in row.SlotNumbers)
                {
                    switch (row.DamageId)
                    {
                        case 1:
                            objectCustomization[slotNumber].Damage1 = damageValue;
                            break;
                        case 2:
                            objectCustomization[slotNumber].Damage2 = damageValue;
                            break;
                        case 3:
                            objectCustomization[slotNumber].Damage3 = damageValue;
                            break;
                    }
                }
            }
        }

        // All these are either bugs in the original Python code, or they're intentionally dead code.
        const short defaultBeetleDispersion = 1024;
        short? beetleDispersion = binaryReader.ReadShortAt(0x0000E3EC).NullIf(defaultBeetleDispersion);
        if (beetleDispersion != null) 
            _logger.LogInformation("Beetle dispersion {BeetleDispersion}", beetleDispersion);

        sbyte? magicalAttackDivider = binaryReader.ReadSByteAt(0x0003A7AD).NullIf<sbyte>(2);
        if (magicalAttackDivider != null)
        {
            _logger.LogInformation("Magical Attack Divider: {MagicalAttackDivider}", magicalAttackDivider);
        }

        var disableMutantLocustAttack = binaryReader.ReadByteAt(0x000042CC) != 0x7F;
        if (disableMutantLocustAttack)
            _logger.LogInformation("Mutant locust attack disabled");

        if (objectCustomization.Any(c => c.HasAnyValue()))
        {
            objectsInfo.ObjectCustomization = objectCustomization;
        }
        
        if (objectsInfo.HasAnyValue())
            return objectsInfo;

        return null;
    }
}