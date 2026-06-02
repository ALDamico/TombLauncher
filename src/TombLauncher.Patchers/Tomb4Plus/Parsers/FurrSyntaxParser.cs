namespace TombLauncher.Patchers.Tomb4Plus.Parsers;

public class FurrSyntaxParser
{
    private const int MaxNops = 128;

    private static readonly byte[][] OneshotOpcodeDefault =
        new List<string>()
        {
            "5351B900009900BB",
            "2F000000",
            "83EB2FC6041901595B66C705FECB4A00",
            "2F00",
        }.Select(Convert.FromHexString)
            .ToArray();

    private static readonly byte[][] OneshotOpcodeRemappedSceneMemory = new List<string>()
        {
            "5351B90000D900BB",
            "2F000000",
            "83EB2FC6041901595B66C705FECB4A00",
            "2F00",
        }.Select(Convert.FromHexString)
        .ToArray();

    private const int FlipeffectTableAddress = 0xC1000;
    private const int FlipeffectDataAddress = 0xc3100;
    private const int RacetimereventDataAddress = 0x00101000;
    private static readonly uint FunctionAddressOffset = BitConverter.ToUInt32(Convert.FromHexString("FBAE7EFF"), 0);
    private static readonly byte[] RacetimerEventNotify = Convert.FromHexString("FF0546777F00");
    private static readonly byte[] RacetimerEventStart = Convert.FromHexString("813D46777F00");

    private const int BaseAddressDefault = 8474880;
    private const int BaseAddressRemappedSceneMemory = 12669184;

    private const int FirstCustomFlipeffect = 47;
    private const int LastCustomFlipeffect = 512;

    private int GetBaseAddress(bool usingRemappedMemory) =>
        usingRemappedMemory ? BaseAddressRemappedSceneMemory : BaseAddressDefault;

    private byte[][] GetOneshotOpcode(bool usingRemappedMemory) =>
        usingRemappedMemory ? OneshotOpcodeRemappedSceneMemory : OneshotOpcodeDefault;

    private List<byte[]> SplitByteArray(byte[] byteArray, List<int> positions, List<int> sizes)
    {
        if (positions.Count != sizes.Count)
            throw new ArgumentOutOfRangeException(nameof(positions), "Positions and size arrays do not match in size");

        var result = new List<byte[]>();

        var start = 0;
        foreach (var (pos, size) in positions.Zip(sizes))
        {
            if (pos > start)
                result.Add(byteArray.Skip(start).Take(pos).ToArray());
            result.Add(byteArray.Skip(pos).Take(size).ToArray());
            start = pos + size;
        }

        if (start < byteArray.Length)
            result.Add(byteArray.Skip(start).ToArray());

        return result;
    }
}