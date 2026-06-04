namespace TombLauncher.Patchers.Tomb4Plus.Models;

internal static class Constants
{
    internal const int ManifestVersion = 0;
    internal const int ExeDefaultSize = 790528;
    internal const int ExeTrepExtendedSize = 1314816;

    internal static readonly string[] TrleVanillaHashes = ["dd351288b437ae4638db9aecca714df4"];

    internal static readonly string[] TrleExtendedHashes =
        ["e2fb8ac766ce0c2bef0e30b20b4e5b38", "0b78a6ecec28ea2725bb163c86b6b747"];

    internal const int ObjectCount = 520;

    internal const byte Tr5BarType = 0x74;
    internal const byte FlatBarType = 0x6c;

    internal static readonly ColorRgb BlackColor = new ColorRgb() { B = 0, G = 0, R = 0 };
    
    // Equipment
    internal const int PistolsId = 349;
    internal const int BinocularsId = 371;
    internal const int CrowbarId = 246;
    internal const int LargeMedipackId = 368;
    internal const int SmallMedipackId = 369;
    internal const int FlareId = 373;
    
    // Weapons
    internal const byte PistolDamage = 1;
    internal const byte UziDamage = 1;
    internal const byte RevolverDamage = 21;
    internal const byte CrossbowBoltDamage = 5;
    internal const byte PistolRate = 9;
    internal const byte UziRate = 3;
    internal const byte RevolverRate = 16;
    internal const byte PistolDispersion = 5;
    internal const byte UziDispersion = 5;
    internal const byte RevolverDispersion = 2;
    internal const byte PistolFlashDuration = 3;
    internal const byte UziFlashDuration = 3;
    internal const byte RevolverFlashDuration = 3;
    internal const byte ShotgunFlashDuration = 3;

    internal const string ColdBreath = "enabled_in_cold_rooms_only";
    internal const short RoomColdFlag = 0x1000;

    internal const byte MemoryRemappedFlag = 0x76;
}