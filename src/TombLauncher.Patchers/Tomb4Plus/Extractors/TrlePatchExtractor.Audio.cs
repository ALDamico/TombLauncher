using TombLauncher.Contracts.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Extractors;

public partial class TrlePatchExtractor
{
    private AudioInfo? ReadAudioInfo(BinaryReader reader, bool isUsingRemappedMemory, PatchBinaryType patchType)
    {
        if (patchType != PatchBinaryType.TrepExe)
            return null;
        var audioInfo = new AudioInfo();

        if (isUsingRemappedMemory)
        {
            var usingBass = reader.IsNopAtRange(0x000F66C0, 0x000F6E7E);
            if (usingBass)
            {
                audioInfo.NewAudioSystem = true;
                audioInfo.OldCdTriggerSystem = false;
            }
        }

        audioInfo.DisableLaraHitSfx =
            reader.CompareDataAtAddress(0x0000B02A, [0x90, 0x90, 0x90, 0x90, 0x90]).NullIf(false);

        const sbyte defaultLaraHitSfx = 50;
        audioInfo.LaraHitSfx = reader.ReadSByteAt(0x0000B029).NullIf(defaultLaraHitSfx);

        audioInfo.DisableNoAmmoSfx =
            reader.CompareDataAtAddress(0x0002D814, [0x90, 0x90, 0x90, 0x90, 0x90]).NullIf(false);

        const sbyte defaultNoAmmoSfx = 48;
        audioInfo.NoAmmoSfx = reader.ReadSByteAt(0x0002D813).NullIf(defaultNoAmmoSfx);

        const byte defaultJeepTrack = 98;
        audioInfo.InsideJeepTrack = reader.ReadByteAt(0x000663FE).NullIf(defaultJeepTrack);

        const byte defaultOutsideJeepTrack = 110;
        audioInfo.OutsideJeepTrack = reader.ReadByteAt(0x00066EAD).NullIf(defaultOutsideJeepTrack);

        const byte defaultSecretTrack = 5;
        audioInfo.SecretTrack = reader.ReadByteAt(0x0004AACC).NullIf(defaultSecretTrack);

        var changeLoopedAudioTrackRange = reader.IsNopAtRange(0x0004BE2C, 0x0004BE3C);
        if (changeLoopedAudioTrackRange)
        {
            const byte defaultLoopedAudioTrack = 105;
            audioInfo.FirstLoopedAudioTrack = reader.ReadByteAt(0x0004BE28).NullIf(defaultLoopedAudioTrack);
        }

        return audioInfo;
    }
}
