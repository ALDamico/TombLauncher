using TombLauncher.Core.Extensions;

namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class VaporInfo
{
    public List<SteamEmitter>? SteamEmittersForOcb { get; set; }
    public List<SteamEmitter>? WhiteSmokeEmittersForOcb { get; set; }
    public List<SteamEmitter>? BlackSmokeEmittersForOcb { get; set; }

    public bool HasAnyValue() => SteamEmittersForOcb.IsNotNullOrEmpty() ||
                                 WhiteSmokeEmittersForOcb.IsNotNullOrEmpty() ||
                                 BlackSmokeEmittersForOcb.IsNotNullOrEmpty();
}