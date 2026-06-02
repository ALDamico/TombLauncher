using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Extractors;

public partial class TrlePatchExtractor
{
    private EnvironmentInfo? ReadEnvironmentInfo(BinaryReader reader, PatchBinaryType patchType)
    {
        if (patchType != PatchBinaryType.TrepExe)
            return null;

        var environmentInfo = new EnvironmentInfo();

        if (reader.ReadByteAt(0x000702A6) == 0xEB && reader.ReadByteAt(0x00070492) == 0xEB &&
            reader.ReadByteAt(0x000706A8) > 0)
        {
            environmentInfo.DisableDistanceLimit = true;
        }

        const int defaultFogEndRange = 20480;
        int? fogEndRange = Convert.ToInt32(reader.ReadFloatAt(0x000B249C)).NullIf(defaultFogEndRange);

        if (fogEndRange != null)
        {
            environmentInfo.FarView = fogEndRange;
            environmentInfo.FogEndRange = fogEndRange;
        }

        var hardClippingRangeFirstValue = reader.ReadUIntAt(0x00075107);
        var hardClippingRangeSecondValue = reader.ReadUIntAt(0x0008CE33);

        // The original Python code doesn't assign the hard clipping range value either.
        const int defaultHardClippingRange = 20480;
        if (hardClippingRangeFirstValue == hardClippingRangeSecondValue)
        {
            if (hardClippingRangeFirstValue != defaultHardClippingRange)
                _logger.LogInformation("Hard Clipping Range: {HardClippingRange}", hardClippingRangeFirstValue);
        }
        else
        {
            _logger.LogWarning("Hard Clipping Range: MISMATCH.");
        }

        const int fogStartRangeDefault = 12288;
        environmentInfo.FogStartRange = Convert.ToInt32(reader.ReadFloatAt(0x000B2498)).NullIf(fogStartRangeDefault);

        return environmentInfo;
    }
}