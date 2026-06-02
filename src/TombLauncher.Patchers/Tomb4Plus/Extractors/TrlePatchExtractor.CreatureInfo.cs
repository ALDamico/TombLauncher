using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Extractors;

public partial class TrlePatchExtractor
{
    private CreatureInfo? ReadCreatureInfo(BinaryReader reader)
    {
        var disabledSentryFlameAttack = reader.IsNopAtRange(0x0003F1F3, 0x0003F1F4);

        if (!disabledSentryFlameAttack)
            return null;

        return new CreatureInfo() { DisableSentryFlameAttack = true };
    }
}