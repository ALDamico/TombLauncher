using System.Text.RegularExpressions;
using TombLauncher.Core.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Patchers.Tomb4Plus.Models;

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
                result.Add(byteArray.Skip(start).Take(pos - start).ToArray());
            result.Add(byteArray.Skip(pos).Take(size).ToArray());
            start = pos + size;
        }

        if (start < byteArray.Length)
            result.Add(byteArray.Skip(start).ToArray());

        return result;
    }

    private int? GetSizeForVariantType(string? type)
    {
        return type switch
        {
            "ASSIGN_BYTE" => 1,
            "SIGNEDBYTE" => 1,
            "UNSIGNEDBYTE" => 1,
            "ASSIGN_INTEGER" => 2,
            "SIGNEDINTEGER" => 2,
            "UNSIGNEDINTEGER" => 2,
            "ASSIGN_LONG" => 4,
            "LONG" => 4,
            "ADDRESS" => 4,
            "FLIPEFFECT" => 4,
            "TIME" => 4,
            "ASSIGN_HEX" => 4, // OG Python code says "check this, might be 2"
            null => null,
            _ => throw new ArgumentException($"Unknown type: {type}", nameof(type))
        };
    }
    
    private object? ReadArg(byte[]? arg, string? type)
    {
        return type switch
        {
            "ASSIGN_BYTE"      => arg![0],
            "UNSIGNEDBYTE"     => arg![0],
            "SIGNEDBYTE"       => (sbyte)arg![0],
            "ASSIGN_INTEGER"   => BitConverter.ToUInt16(arg!, 0),
            "UNSIGNEDINTEGER"  => BitConverter.ToUInt16(arg!, 0),
            "SIGNEDINTEGER"    => BitConverter.ToInt16(arg!, 0),
            "ASSIGN_LONG"      => BitConverter.ToUInt32(arg!, 0),
            "ADDRESS"          => BitConverter.ToUInt32(arg!, 0),
            "FLIPEFFECT"       => BitConverter.ToUInt32(arg!, 0),
            "TIME"             => BitConverter.ToUInt32(arg!, 0),
            "ASSIGN_HEX"       => BitConverter.ToUInt32(arg!, 0), // Python: might be 2
            "LONG"             => BitConverter.ToInt32(arg!, 0),
            null               => null,
            _                  => throw new ArgumentException($"Unknown type: {type}")
        };
    }

    public FurrCommand CreateFlipeffectTableEntryForOpcode(FurrOpcode opcode, byte[]? firstArg, byte[]? secondArg)
    {
        object? firstArgTyped;
        object? secondArgTyped;
        if (opcode.ReverseArgs)
        {
            firstArgTyped = ReadArg(firstArg, opcode.SecondArgType);
            secondArgTyped = ReadArg(secondArg, opcode.FirstArgType);
        }
        else
        {
            firstArgTyped = ReadArg(firstArg, opcode.FirstArgType);
            secondArgTyped = ReadArg(secondArg, opcode.SecondArgType);
        }
        
        return opcode.ReverseArgs
            ? new FurrCommand { FunctionName = opcode.FunctionName, FirstArg = secondArgTyped, SecondArg = firstArgTyped }
            : new FurrCommand { FunctionName = opcode.FunctionName, FirstArg = firstArgTyped, SecondArg = secondArgTyped };
    }
    
    private List<byte[]> GetSplitByteArraysWithArgs(byte[] myBytes, string? firstArgType, string? secondArgType,
        int firstArgPos, int secondArgPos, bool firstArgIsLocalOffset, bool secondArgIsLocalOffset, bool reverseArgs)
    {
        var positionArr = new List<int>();
        var sizesArr = new List<int>();

        var firstArgVariantSize = GetSizeForVariantType(firstArgType);
        if (firstArgVariantSize != null)
        {
            if (firstArgIsLocalOffset)
                firstArgPos -= 1;
            positionArr.Add(firstArgPos);
            sizesArr.Add(firstArgVariantSize.Value);
        }

        var secondArgVariantSize = GetSizeForVariantType(secondArgType);
        if (secondArgVariantSize != null)
        {
            if (secondArgIsLocalOffset)
                secondArgPos -= 1;

            // Workaround for both variants being aligned next to each other
            if (firstArgVariantSize != null && secondArgPos == firstArgPos + firstArgVariantSize.Value)
            {
                positionArr.Add(secondArgPos);
                sizesArr.Add(0);
            }

            positionArr.Add(secondArgPos);
            sizesArr.Add(secondArgVariantSize.Value);
        }

        if (reverseArgs)
        {
            positionArr.Reverse();
            sizesArr.Reverse();
        }

        return positionArr.Count > 0
            ? SplitByteArray(myBytes, positionArr, sizesArr)
            : [myBytes];
    }

    private List<int> FindAllAddresses(string s)
    {
        var pattern = "//";
        var counted = 0;
        var addressArray = new List<int>();

        foreach (var match in Regex.EnumerateMatches(s, pattern))
        {
            var num = match.Index;
            addressArray.Add((num - counted) / 2);
            counted += 2;
        }

        return addressArray;
    }

    private async Task<List<FurrOpcode>> LoadSyntaxFile(SyntaxFile syntaxFile, CancellationToken cancellationToken)
    {
        string fileName;
        switch (syntaxFile)
        {
            case SyntaxFile.Early:
                fileName = "syntaxEarly.fln";
                break;
            case SyntaxFile.Trep:
                fileName = "syntaxTREP.fln";
                break;
            case SyntaxFile.TrLarson:
                fileName = "syntaxT4L.fln";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(syntaxFile), syntaxFile, null);
        }

        var opcodes = new List<FurrOpcode>();

        fileName = $"TombLauncher.Patchers.Tomb4Plus.Furr.{fileName}";
        var assembly = GetType().Assembly;
        await using var stream = assembly.GetManifestResourceStream(fileName)!;
        using var reader = new StreamReader(stream);
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (line.IsNullOrWhiteSpace() || line.StartsWith(';') || line.StartsWith('!'))
                continue;

            var tokens = line.Split(' ');
            var addressTable = FindAllAddresses(tokens[0]);
            var assemblyString = tokens[0].Replace("//", "").Replace(" ", "");
            var firstArgPos = int.Parse(tokens[1]);
            var secondArgPos = int.Parse(tokens[2]);
            var functionName = tokens[3];

            var firstArgIsLocalOffset = false;
            var secondArgsIsLocalOffset = false;

            if (functionName == "CALL")
                firstArgIsLocalOffset = true;

            string? firstArgType = null;
            string? secondArgType = null;

            if (tokens.Length > 4)
            {
                firstArgType = tokens[4];
                if (tokens.Length > 5)
                    secondArgType = tokens[5];
            }

            var reverseArgs = secondArgPos < firstArgPos && secondArgType != null;
            var assemblyBytes = Convert.FromHexString(assemblyString);
            var totalLength = assemblyBytes.Length;

            var splitByteArrays = GetSplitByteArraysWithArgs(assemblyBytes, firstArgType, secondArgType, firstArgPos,
                secondArgPos, firstArgIsLocalOffset, secondArgsIsLocalOffset, reverseArgs);

            var opcode = new FurrOpcode()
            {
                FunctionName = functionName,
                AddressTable = addressTable,
                ByteArrays = splitByteArrays,
                FirstArgType = firstArgType,
                SecondArgType = secondArgType,
                ReverseArgs = reverseArgs,
                TotalLength = totalLength
            };
            opcodes.Add(opcode);
        }

        return opcodes;
    }

    private byte[] ConvertLocalAddressesToGlobal(byte[] commandBytes, int commandBasePosition,
        List<int> addressTable, bool isUsingRemappedMemory)
    {
        var commandBytesArray = new byte[commandBytes.Length];
        commandBytes.CopyTo(commandBytesArray);

        foreach (var addressOffset in addressTable)
        {
            if (addressOffset > commandBytes.Length - 4)
                continue;

            var localAddressByteArray = commandBytes[addressOffset..(addressOffset + 4)];
            var localAddressAsInt = BitConverter.ToUInt32(localAddressByteArray);
            var globalAddressAsInt = 0L;
            if (isUsingRemappedMemory)
                globalAddressAsInt = (localAddressAsInt - 0xff413000) + 0x00028105 - 0x2100 + (addressOffset - 1) +
                                     commandBasePosition;
            else
                globalAddressAsInt = (localAddressAsInt - 0xff813000) + 0x00028105 - 0x2100 + (addressOffset - 1) +
                                     commandBasePosition;

            if (globalAddressAsInt > 0xffffffff)
                globalAddressAsInt -= 0xffffffff;
            else if (globalAddressAsInt < 0)
                globalAddressAsInt += 0xffffffff;
            
            var globalAddressByteArray = BitConverter.GetBytes((uint)globalAddressAsInt);
            Array.Reverse(globalAddressByteArray);

            Array.Copy(globalAddressByteArray, 0, commandBytesArray, addressOffset, 4);
        }

        return commandBytesArray;
    }
}