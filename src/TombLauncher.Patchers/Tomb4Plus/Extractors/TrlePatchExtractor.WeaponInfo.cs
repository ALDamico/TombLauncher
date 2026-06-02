using TombLauncher.Contracts.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Extractors;

public partial class TrlePatchExtractor
{
    private WeaponInfo? ReadWeaponInfo(BinaryReader reader, PatchBinaryType patchType)
    {
        if (patchType != PatchBinaryType.TrepExe)
            return null;

        var weaponInfo = new WeaponInfo();

        weaponInfo.PistolDamage = reader.ReadByteAt(0x000AB876).NullIf(Constants.PistolDamage);
        weaponInfo.UziDamage = reader.ReadByteAt(0x000AB8C2).NullIf(Constants.UziDamage);
        weaponInfo.RevolverDamage = reader.ReadByteAt(0x000AB89C).NullIf(Constants.RevolverDamage);
        weaponInfo.PistolRate = reader.ReadByteAt(0x000AB877).NullIf(Constants.PistolRate);
        weaponInfo.UziRate = reader.ReadByteAt(0x000AB8C3).NullIf(Constants.UziRate);
        weaponInfo.RevolverRate = reader.ReadByteAt(0x000AB89D).NullIf(Constants.RevolverRate);
        weaponInfo.PistolDispertion = reader.ReadByteAt(0x000AB871).NullIf(Constants.PistolDispersion);
        weaponInfo.UziDispertion = reader.ReadByteAt(0x000AB8BD).NullIf(Constants.UziDispersion);
        weaponInfo.RevolverDispertion = reader.ReadByteAt(0x000AB897).NullIf(Constants.RevolverDispersion);
        weaponInfo.PistolFlashDuration = reader.ReadByteAt(0x000AB878).NullIf(Constants.PistolFlashDuration);
        weaponInfo.UziFlashDuration = reader.ReadByteAt(0x000AB8C4).NullIf(Constants.UziFlashDuration);
        weaponInfo.RevolverFlashDuration = reader.ReadByteAt(0x000AB89E).NullIf(Constants.RevolverFlashDuration);
        weaponInfo.ShotgunFlashDuration = reader.ReadByteAt(0x000AB8EA).NullIf(Constants.ShotgunFlashDuration);
        weaponInfo.CrossbowBoltDamage = reader.ReadByteAt(0x000AB934).NullIf(Constants.CrossbowBoltDamage);

        if (weaponInfo.HasAnyValue())
            return weaponInfo;

        return null;
    }
}