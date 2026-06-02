using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Extractors;

public partial class TrlePatchExtractor
{
    private StatInfo? ReadStatInfo(BinaryReader reader, PatchBinaryType patchType)
    {
        if (patchType != PatchBinaryType.TrepExe)
            return null;

        var statInfo = new StatInfo();

        var secretCountStr = reader.GetFixedStringAt(0x000B1785, 2);
        const int defaultSecretCount = 70;
        if (int.TryParse(secretCountStr, out var secretCount))
        {
            if (secretCount != defaultSecretCount)
                statInfo.SecretCount = secretCount;
        }

        var equipmentModifiers = new List<EquipmentModifier>();
        if (reader.IsNopAtRange(0x0005B426, 0x0005B42B))
            equipmentModifiers.Add(new EquipmentModifier(){ObjectId = Constants.PistolsId, Amount = 0});

        var hasBinoculars = reader.ReadByteAt(0x0005B455) > 0;
        if (!hasBinoculars)
            equipmentModifiers.Add(new EquipmentModifier(){ObjectId = Constants.BinocularsId, Amount = 0});
        
        if (reader.IsNopAtRange(0x0005B475, 0x0005B476))
            equipmentModifiers.Add(new EquipmentModifier(){ObjectId = Constants.CrowbarId, Amount = 1});

        const short defaultLargeMedipackCount = 1;
        var largeMedipackCount = reader.ReadShortAt(0x0005B469);
        if (largeMedipackCount > defaultLargeMedipackCount)
            equipmentModifiers.Add(new EquipmentModifier(){ObjectId = Constants.LargeMedipackId, Amount = largeMedipackCount});

        var smallMedipackCount = 3;
        var flareCount = 3;
        if (reader.ReadByteAt(0x0005B443) == 0xB4)
        {
            smallMedipackCount = reader.ReadByteAt(0x0005B446);
            flareCount = reader.ReadByteAt(0x0005B444);
        }
        else
        {
            smallMedipackCount = reader.ReadIntAt(0x0005B444);
            flareCount = reader.ReadIntAt(0x0005B444);
        }
        
        const int defaultSmallMedipackCount = 3;
        const int defaultFlareCount = 3;
        if (smallMedipackCount != defaultSmallMedipackCount)
            equipmentModifiers.Add(new EquipmentModifier(){ObjectId = Constants.SmallMedipackId, Amount = smallMedipackCount});
        
        if (flareCount != defaultFlareCount)
            equipmentModifiers.Add(new EquipmentModifier(){ObjectId = Constants.FlareId, Amount = flareCount});

        if (equipmentModifiers.Count > 0)
            statInfo.EquipmentModifiers = equipmentModifiers;

        if (statInfo.HasAnyValue())
            return statInfo;

        return null;
    }
}