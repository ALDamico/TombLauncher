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
}