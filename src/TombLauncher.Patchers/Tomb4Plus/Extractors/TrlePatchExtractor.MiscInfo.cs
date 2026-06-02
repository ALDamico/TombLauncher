using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Extractors;

public partial class TrlePatchExtractor
{
    private MiscInfo? ReadMiscInfo(BinaryReader reader, PatchBinaryType patchType)
    {
        const byte defaultBlinkInterval = 5;
        const byte defaultLegendTimer = 150;
        var miscInfo = new MiscInfo();
        miscInfo.TextOrCriticalBarBlinkInterval = reader.ReadByteAt(0x000521B0).NullIf(defaultBlinkInterval);
        miscInfo.LegendTimer = reader.ReadByteAt(0x00050F59).NullIf(defaultLegendTimer);

        if (patchType == PatchBinaryType.TrepExe)
        {
            var removeLookTransparency = reader.ReadByteAt(0x0001D0C0) == 0xEB;
            if (removeLookTransparency)
                _logger.LogInformation("Look Transparency disabled");

            if (reader.IsNopAtRange(0x000160ED, 0x000160EE))
                miscInfo.LaraImpalesOnSpikes = true;

            const ushort defaultLowerShatterThreshold = 50;
            const ushort defaultUpperShatterThreshold = 58;

            var lowerStaticShatterThreshold = reader.ReadUShortAt(0x0004D013);
            var upperStaticShatterThreshold = reader.ReadUShortAt(0x0004D019);

            if (lowerStaticShatterThreshold != defaultLowerShatterThreshold ||
                upperStaticShatterThreshold != defaultUpperShatterThreshold)
            {
                _logger.LogInformation("Static Shatter Range: {LowerThreshold}-{UpperThreshold}",
                    lowerStaticShatterThreshold, upperStaticShatterThreshold);
            }

            if (reader.CompareDataAtAddress(0x00014044, [0xF2]))
                miscInfo.DartsPoisonFix = true;

            const short defaultPoisonDartValue = 160;
            var poisonDartValue = reader.ReadShortAt(0x00014048);
            if (poisonDartValue != defaultPoisonDartValue)
                _logger.LogInformation("Poison Dart Poison Value: {PoisonValue}", poisonDartValue);

            var fixHolsters = !(reader.IsNopAtRange(0x0002B7C1, 0x0002B7CB) ||
                                reader.IsNopAtRange(0x0002B845, 0x0002B84F));
            if (fixHolsters)
                _logger.LogInformation("Fix Holsters: true");

            miscInfo.AlwaysExitFromStatisticsScreen = (reader.ReadByteAt(0x0007AD9B) == 0xC6 &&
                                                       reader.CompareDataAtAddress(0x0007ADA2,
                                                       [
                                                           0xE8, 0x99, 0x36, 0xFE, 0xFF, 0x83, 0xC4, 0x0C, 0xB0, 0x01,
                                                           0xC3
                                                       ])).NullIf(false);
        }

        miscInfo.DisableMotorbikeHeadlights = (reader.ReadByteAt(0x000639F0) == 0xC3).NullIf(false);

        if (miscInfo.HasAnyValue())
            return miscInfo;

        return null;
    }
}