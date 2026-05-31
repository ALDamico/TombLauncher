using System.Text;
using TombLauncher.Core.Extensions;
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

    public static string ReadCString(this BinaryReader reader)
    {
        var sb = new StringBuilder();

        byte b;
        do
        {
            b = reader.ReadByte();
            if (b != 0)
                sb.Append((char)b);
        } while (b != 0);
        
        return sb.ToString();
    }

    public static void SkipBytes(this BinaryReader reader, int bytes)
    {
        reader.BaseStream.Seek(bytes, SeekOrigin.Current);
    }

    public static ColorRgb GetBgrColorAtAddress(this BinaryReader reader, long startAddress)
    {
        reader.Seek(startAddress);
        return reader.ReadBgr();
    }

    public static ColorRgb GetRgbColorAtAddress(this BinaryReader reader, long startAddress)
    {
        reader.Seek(startAddress);
        return reader.ReadRgb();
    }

    public static sbyte ReadSByteAt(this BinaryReader reader, long startAddress)
    {
        reader.Seek(startAddress);
        return reader.ReadSByte();
    }

    public static byte ReadByteAt(this BinaryReader reader, long startAddress)
    {
        reader.Seek(startAddress);
        return reader.ReadByte();
    }

    public static short ReadShortAt(this BinaryReader reader, long startAddress)
    {
        reader.Seek(startAddress);
        return reader.ReadInt16();
    }

    public static ushort ReadUShortAt(this BinaryReader reader, long startAddress)
    {
        reader.Seek(startAddress);
        return reader.ReadUInt16();
    }

    public static int ReadIntAt(this BinaryReader reader, long startAddress)
    {
        reader.Seek(startAddress);
        return reader.ReadInt32();
    }

    public static uint ReadUIntAt(this BinaryReader reader, long startAddress)
    {
        reader.Seek(startAddress);
        return reader.ReadUInt32();
    }

    public static float ReadFloatAt(this BinaryReader reader, long startAddress)
    {
        reader.Seek(startAddress);
        return reader.ReadSingle();
    }

    public static string GetFixedStringAt(this BinaryReader reader, long startAddress, int length)
    {
        reader.Seek(startAddress);
        var bytes = reader.ReadBytes(length);
        return bytes.GetNullTerminatedString();
    }

    public static void Seek(this BinaryReader reader, long startAddress)
    {
        reader.BaseStream.Seek(startAddress, SeekOrigin.Begin);
    }
}