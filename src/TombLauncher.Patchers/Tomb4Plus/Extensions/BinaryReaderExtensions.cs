using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Extensions;

public static class BinaryReaderExtensions
{
    private const byte NoOp = 0x90;
    
    public static bool CompareDataAtAddress(this BinaryReader reader, long startAddress, byte[] dataBlock)
    {
        reader.Seek(startAddress);
        var readBytes = reader.ReadBytes(dataBlock.Length);

        return dataBlock.SequenceEqual(readBytes);
    }

    public static bool IsNopAtRange(this BinaryReader reader, long startAddress, long endAddress)
    {
        reader.Seek(startAddress);

        var readBytes = reader.ReadBytes((int)(endAddress - startAddress) + 1);
        return readBytes.All(b => b == NoOp);
    }

    public static ColorRgb ReadBgr(this BinaryReader reader)
    {
        var bgrBytes = reader.ReadBytes(3);

        return new ColorRgb()
        {
            B = bgrBytes[0],
            G = bgrBytes[1],
            R = bgrBytes[2]
        };
    }

    public static ColorRgb ReadRgb(this BinaryReader reader)
    {
        var rgbBytes = reader.ReadBytes(3);

        return new ColorRgb()
        {
            R = rgbBytes[0],
            G = rgbBytes[1],
            B = rgbBytes[2]
        };
    }

    public static void Seek(this BinaryReader reader, long startAddress)
    {
        reader.BaseStream.Seek(startAddress, SeekOrigin.Begin);
    }
}