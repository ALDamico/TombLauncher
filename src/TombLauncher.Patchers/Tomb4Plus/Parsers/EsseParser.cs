using System.Text;
using TombLauncher.Patchers.Tomb4Plus.Extensions;

namespace TombLauncher.Patchers.Tomb4Plus.Parsers;

public class EsseParser
{
    private void ReadData(BinaryReader reader, Dictionary<string, object> d, string keyName, int baseOffset, int offset,
        int maxBlockSize, string type, int stringWidth, object defaultValue)
    {
        object? data;
        
        reader.Seek(baseOffset + offset);
        
        switch (type)
        {
            case "FLOAT":
                if (offset + 4 <= maxBlockSize)
                    data = reader.ReadSingle();
                else
                    data = BitConverter.Int32BitsToSingle((int)defaultValue);
                break;
            case "BOOL":
                if (offset + 1 <= maxBlockSize)
                    data = reader.ReadByte() != 0;
                else
                    data = (bool)defaultValue;
                break;
            case "BYTE":
                if (offset + 1 <= maxBlockSize)
                    data = reader.ReadByte();
                else 
                    data = (byte)defaultValue;
                break;
            case "BYTEI":
                if (offset + 1 <= maxBlockSize)
                    data = reader.ReadSByte();
                else
                    data = (sbyte)defaultValue;
                break;
            case "WORD":
                if (offset + 2 <= maxBlockSize)
                    data = reader.ReadUInt16();
                else
                    data = (ushort)defaultValue;
                break;
            case "WORDI":
                if (offset + 2 <= maxBlockSize)
                    data = reader.ReadInt16();
                else
                    data = (short)defaultValue;
                break;
            case "DWORD":
                if (offset + 4 <= maxBlockSize)
                    data = reader.ReadUInt32();
                else
                    data = (uint)defaultValue;
                break;
            case "HEX":
                if (offset + 3 <= maxBlockSize)
                {
                    var bytes = reader.ReadBytes(3);
                    data = $"#{bytes[0]:x2}{bytes[1]:x2}{bytes[2]:x2}";
                }
                else
                    data = "#000000";

                break;
            case "STRING":
                data = Encoding.ASCII.GetString(reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position)));
                break;
            case "FIXED_STRING":
                if (offset + stringWidth <= maxBlockSize)
                    data = Encoding.ASCII.GetString(reader.ReadBytes(stringWidth));
                else
                    data = defaultValue.ToString();
                break;
            default:
                data = defaultValue;
                break;
        }

        if (data != null)
            d[keyName] = data;
    }
}