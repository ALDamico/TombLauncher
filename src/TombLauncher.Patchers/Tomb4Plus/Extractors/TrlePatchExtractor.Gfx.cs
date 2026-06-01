using TombLauncher.Contracts.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Extractors;

public partial class TrlePatchExtractor
{
    private GfxInfo? ReadGfxInfo(BinaryReader reader, PatchBinaryType patchType)
    {
        var gfxInfo = new GfxInfo();

        gfxInfo.BloodInfo = ReadGfxBloodInfo(reader);
        gfxInfo.VaporInfo = ReadGfxVaporInfo(reader, patchType);

        if (gfxInfo.HasAnyValue())
            return gfxInfo;

        return null;
    }

    private BloodInfo? ReadGfxBloodInfo(BinaryReader reader)
    {
        var bloodInfo = new BloodInfo();

        const byte defaultBloodSize = 0x08;
        bloodInfo.BloodSize = reader.ReadByteAt(0x00038A18).NullIf(defaultBloodSize);
        const byte defaultBloodIntensity = 0x30;
        bloodInfo.BloodIntensity = reader.ReadByteAt(0x0003891C).NullIf(defaultBloodIntensity);
        const byte defaultBloodSpeed = 0x05;
        bloodInfo.BloodSpeed = reader.ReadByteAt(0x00038894).NullIf(defaultBloodSpeed);
        // This is probably a bug in the original Python code. Maybe Saracen meant to assign it to blood_spread_intensity?
        bloodInfo.BloodIntensity = reader.ReadByteAt(0x0003892C).NullIf<byte>(0x18);
        bloodInfo.BloodSpreadFactorX = reader.ReadByteAt(0x000389A8).NullIf<byte>(0x07);
        bloodInfo.BloodSpreadFactorY = reader.ReadByteAt(0x000389AB).NullIf<byte>(0x07);

        if (bloodInfo.HasAnyValue())
            return bloodInfo;

        return null;
    }

    private VaporInfo? ReadGfxVaporInfo(BinaryReader reader, PatchBinaryType patchType)
    {
        if (patchType != PatchBinaryType.FlepExe && patchType != PatchBinaryType.FlepExternalBinary)
            return null;

        var extendedVaporEmitter = reader.IsNopAtRange(0x000C53E0, 0x000C5C83);
        if (extendedVaporEmitter)
            return null;
        
        reader.Seek(0x000C5B21);

        var vaporInfo = new VaporInfo();

        var steamEmitter = ReadVaporCustomization(reader, 0x000C5498, 0);
        vaporInfo.SteamEmittersForOcb = new();
        for (var i = 0; i < 15; i++)
            vaporInfo.SteamEmittersForOcb.Add(steamEmitter);
        
        reader.Seek(0x000C54E0);
        vaporInfo.WhiteSmokeEmittersForOcb = new();
        for (var i = 0; i < 15; i++)
        {
            var vaporCustomization = ReadVaporCustomization(reader, 0x000C5408, i);
            vaporInfo.WhiteSmokeEmittersForOcb.Add(vaporCustomization);
        }
        
        reader.Seek(0x000C5805);
        vaporInfo.BlackSmokeEmittersForOcb = new();
        for (var i = 0; i < 15; i++)
        {
            var vaporCustomization = ReadVaporCustomization(reader, 0x000C545C, i);
            vaporInfo.BlackSmokeEmittersForOcb.Add(vaporCustomization);
        }

        if (vaporInfo.HasAnyValue())
            return vaporInfo;

        return null;
    }

    private SteamEmitter ReadVaporCustomization(BinaryReader reader, int jumpAddress, int index)
    {
        var steamEmitter = new SteamEmitter();

        steamEmitter.StartColor = reader.ReadRgb();
        steamEmitter.StartTime = reader.ReadSByte();
        reader.SkipBytes(1);
        steamEmitter.EndColor = reader.ReadRgb();
        steamEmitter.EndTime = reader.ReadSByte();
        reader.SkipBytes(1);
        steamEmitter.BlendingMode = reader.ReadSByte();
        reader.SkipBytes(1);
        steamEmitter.Lifetime = reader.ReadSByte();
        reader.SkipBytes(1);
        steamEmitter.SizeVariationLowerByte = reader.ReadByte();
        reader.SkipBytes(1);
        steamEmitter.SizeVariationHigherByte = reader.ReadByte();
        reader.SkipBytes(1);
        steamEmitter.SizeMultiplier = reader.ReadByte();
        reader.SkipBytes(1);
        steamEmitter.Rotation = reader.ReadByte();
        reader.SkipBytes(1);
        steamEmitter.Flags = reader.ReadUInt16();
        reader.SkipBytes(1);
        steamEmitter.SpriteId = reader.ReadByte();
        reader.SkipBytes(1);
        steamEmitter.HorizontalSpeed = reader.ReadSByte();
        reader.SkipBytes(1);
        steamEmitter.HorizontalCurve = reader.ReadSByte();
        reader.SkipBytes(1);
        steamEmitter.VerticalSpeed1 = reader.ReadInt32();
        reader.SkipBytes(1);
        steamEmitter.VerticalSpeed2 = reader.ReadSByte();

        var lastPosition = reader.BaseStream.Position;

        reader.Seek(jumpAddress + index * 4);
        steamEmitter.SpawnInterval = reader.ReadSByte();
        reader.Seek(lastPosition + 10);

        return steamEmitter;
    }
}
