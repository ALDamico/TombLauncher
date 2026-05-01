using System.Text;

namespace TombLauncher.Patchers.Gameflows;

public class TpcStringArray
{
    public TpcStringArray(int count)
    {
        Count = count;
        Offsets = new ushort[Count];
    }
    public int Count { get; set; }
    public ushort[] Offsets { get; }
    public int TotalSize
    {
        get;
        set
        {
            field = value;
            var newArr = new byte[value];
            if (Data != null)
                Array.Copy(Data, newArr, value);

            Data = newArr;
        }
    }

    public byte[]? Data { get; private set; }

    public string DecodeString(byte xorKey)
    {
        var data = Data ?? [];
        if (xorKey != 0)
        {
            data = data.Select(b => (byte)(b ^ xorKey)).ToArray();
        }

        return Encoding.ASCII.GetString(data);
    }
}