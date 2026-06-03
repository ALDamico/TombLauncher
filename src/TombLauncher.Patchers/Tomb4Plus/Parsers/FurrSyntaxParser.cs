using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TombLauncher.Core.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Parsers;

public class FurrSyntaxParser
{
    private readonly ILogger<FurrSyntaxParser> _logger;
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
    private const int RacetimerEventDataAddress = 0x00101000;
    private static readonly uint FunctionAddressOffset = BitConverter.ToUInt32(Convert.FromHexString("FBAE7EFF"), 0);
    private static readonly byte[] RacetimerEventNotify = Convert.FromHexString("FF0546777F00");
    private static readonly byte[] RacetimerEventStart = Convert.FromHexString("813D46777F00");

    private const int BaseAddressDefault = 8474880;
    private const int BaseAddressRemappedSceneMemory = 12669184;

    private const int FirstCustomFlipeffect = 47;
    private const int LastCustomFlipeffect = 512;

    public FurrSyntaxParser(ILogger<FurrSyntaxParser> logger)
    {
        _logger = logger;
    }

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
            "ASSIGN_BYTE" => arg![0],
            "UNSIGNEDBYTE" => arg![0],
            "SIGNEDBYTE" => (sbyte)arg![0],
            "ASSIGN_INTEGER" => BitConverter.ToUInt16(arg!, 0),
            "UNSIGNEDINTEGER" => BitConverter.ToUInt16(arg!, 0),
            "SIGNEDINTEGER" => BitConverter.ToInt16(arg!, 0),
            "ASSIGN_LONG" => BitConverter.ToUInt32(arg!, 0),
            "ADDRESS" => BitConverter.ToUInt32(arg!, 0),
            "FLIPEFFECT" => BitConverter.ToUInt32(arg!, 0),
            "TIME" => BitConverter.ToUInt32(arg!, 0),
            "ASSIGN_HEX" => BitConverter.ToUInt32(arg!, 0), // Python: might be 2
            "LONG" => BitConverter.ToInt32(arg!, 0),
            null => null,
            _ => throw new ArgumentException($"Unknown type: {type}")
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
            ? new FurrCommand
                { FunctionName = opcode.FunctionName, FirstArg = secondArgTyped, SecondArg = firstArgTyped }
            : new FurrCommand
                { FunctionName = opcode.FunctionName, FirstArg = firstArgTyped, SecondArg = secondArgTyped };
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

    private List<(FurrOpcode Opcode, byte[]? FirstArg, byte[]? SecondArg)> ScanForPossibleCommands(
        BinaryReader reader, List<FurrOpcode> opcodeList, long commandPosition, bool isUsingRemappedMemory)
    {
        var possibleCommands = new List<(FurrOpcode, byte[]?, byte[]?)>();

        byte[] lastDataBuffer = [];

        foreach (var opcode in opcodeList)
        {
            reader.Seek(commandPosition);

            var byteArrays = opcode.ByteArrays;
            var originalDataBuffer = reader.ReadBytes(opcode.TotalLength);
            var addressedFixedDataBuffer = ConvertLocalAddressesToGlobal(originalDataBuffer, FlipeffectDataAddress,
                opcode.AddressTable, isUsingRemappedMemory);

            var offsets = new int[byteArrays.Count + 1];
            for (var i = 0; i < byteArrays.Count; i++)
                offsets[i + 1] = offsets[i] + byteArrays[i].Length;

            byte[]? firstArg = null;
            byte[]? secondArg = null;
            var matched = true;

            for (var i = 0; i < byteArrays.Count; i++)
            {
                var start = offsets[i];
                var end = offsets[i + 1];
                if (i % 2 == 0) // fixed segment — must match
                {
                    if (!byteArrays[i].SequenceEqual(addressedFixedDataBuffer[start..end]))
                    {
                        matched = false;
                        break;
                    }
                }
                else // argument slot — extract from original
                {
                    var arg = originalDataBuffer[start..end];
                    if (i == 1) firstArg = arg;
                    else if (i == 3) secondArg = arg;
                }
            }

            lastDataBuffer = originalDataBuffer;

            if (matched)
                possibleCommands.Add((opcode, firstArg, secondArg));
        }

        if (possibleCommands.Count == 0)
            _logger.LogWarning("Could not find any commands for buffer: {BufferStr}",
                Convert.ToHexString(lastDataBuffer));


        return possibleCommands;
    }

    private FurrOptimalCommand ScanForOptimalCommand(BinaryReader reader, List<FurrOpcode> opcodeList,
        long commandPosition, bool isUsingRemappedMemory)
    {
        var possibleCommands = ScanForPossibleCommands(reader, opcodeList, commandPosition, isUsingRemappedMemory);

        if (possibleCommands.Count > 0)
        {
            var finalCommandList = possibleCommands.OrderByDescending(x => x.Opcode.ByteArrays.Count)
                .ThenBy(x => x.Opcode.TotalLength);

            var finalCommand = finalCommandList.LastOrDefault();
            var flipeffectCommandTableEntry = CreateFlipeffectTableEntryForOpcode(finalCommand.Opcode,
                finalCommand.FirstArg, finalCommand.SecondArg);

            var wasNop = finalCommand.Opcode.FunctionName == "NOP";
            commandPosition += finalCommand.Opcode.TotalLength;
            reader.Seek(commandPosition);

            return new FurrOptimalCommand() { NewCommand = flipeffectCommandTableEntry, WasNop = wasNop };
        }

        return new FurrOptimalCommand() { NewCommand = null, WasNop = false };
    }

    private List<List<FurrCommand>> ExtractRacetimerEventsFromExe(BinaryReader reader, List<FurrOpcode> opcodeList,
        bool isUsingRemappedMemory)
    {
        reader.Seek(RacetimerEventDataAddress);

        var racetrackEvents = new List<List<FurrCommand>>();

        var firstBlock = reader.ReadBytes(RacetimerEventNotify.Length);
        if (firstBlock.SequenceEqual(RacetimerEventNotify))
        {
            firstBlock = reader.ReadBytes(RacetimerEventStart.Length);
            if (firstBlock.SequenceEqual(RacetimerEventStart))
            {
                var time = reader.ReadInt32();
                reader.SkipBytes(6);
                var currentCommandList = new List<FurrCommand>();
                var nopCount = 0;

                while (true)
                {
                    if (nopCount > MaxNops)
                    {
                        racetrackEvents.Add(currentCommandList);
                        break;
                    }

                    var pos = reader.BaseStream.Position;
                    var testEnd = reader.ReadBytes(RacetimerEventStart.Length);
                    if (testEnd.SequenceEqual(RacetimerEventStart))
                    {
                        racetrackEvents.Add(currentCommandList);
                        currentCommandList = new List<FurrCommand>();
                        time = reader.ReadInt32();
                        reader.SkipBytes(6);
                        continue;
                    }

                    reader.Seek(pos);

                    var commandResult = ScanForOptimalCommand(reader, opcodeList, pos, isUsingRemappedMemory);
                    if (commandResult.WasNop)
                        nopCount++;
                    else
                    {
                        currentCommandList.Add(commandResult.NewCommand ?? FurrCommand.UnknownCommand);
                        nopCount = 0;
                    }
                }
            }
        }

        return racetrackEvents;
    }

    private List<List<FurrOptimalCommand>> ExtractFlipEffectTableFromExe(BinaryReader reader, List<FurrOpcode> opcodeList,
        bool isUsingRemappedMemory)
    {
        var offsetTable = new List<long>();
        reader.Seek(FlipeffectTableAddress);

        for (var i = 0; i < LastCustomFlipeffect - FirstCustomFlipeffect; i++)
        {
            var address = reader.ReadUInt32();
            if (address == 0)
                offsetTable.Add(-1);
            else
                offsetTable.Add(address - GetBaseAddress(isUsingRemappedMemory));
        }
        
        // Add an extra entry for testing
        offsetTable.Add(-1);
        var flipeffectTable = new List<List<FurrOptimalCommand>>();
        for (var i = 0; i < LastCustomFlipeffect - FirstCustomFlipeffect; i++)
        {
            var flipeffectCommandTable = new List<FurrOptimalCommand>();
            var nopCount = 0;
            if (offsetTable[i] != -1)
            {
                reader.Seek(FlipeffectDataAddress + offsetTable[i]);

                var commandPosition = reader.BaseStream.Position;
                while (true)
                {
                    // Indicates we've likely reached the end
                    if (nopCount > MaxNops)
                        break;

                    commandPosition = reader.BaseStream.Position;

                    if (offsetTable[i + 1] > 0)
                    {
                        if (commandPosition - FlipeffectDataAddress >= offsetTable[i + 1])
                        {
                            break;
                        }
                    }

                    var commandResult =
                        ScanForOptimalCommand(reader, opcodeList, commandPosition, isUsingRemappedMemory);
                    if (commandResult.WasNop)
                        nopCount++;
                    else
                    {
                        nopCount = 0;
                        if (commandResult.NewCommand != null)
                        {
                            flipeffectCommandTable.Add(commandResult);
                            if (commandResult.NewCommand.FunctionName == "RETN")
                                break;
                        }
                        else
                            flipeffectCommandTable.Add(new FurrOptimalCommand(){NewCommand = FurrCommand.UnknownCommand, WasNop = false});

                        nopCount = 0;
                    }
                }
            }

            flipeffectTable.Add(flipeffectCommandTable);
        }
        
        for (var i = 0; i < flipeffectTable.Count; i++)
        {
            if (flipeffectTable[i].Count > 0)
            {
                _logger.LogInformation("FlipEffect: {Idx}", i + FirstCustomFlipeffect);
                foreach (var command in flipeffectTable[i])
                {
                    _logger.LogInformation(command.NewCommand?.FunctionName);
                }
            }
            else
            {
                _logger.LogWarning("Could not find any commands for flipeffect: {Idx}", i + FirstCustomFlipeffect);
            }
        }

        return flipeffectTable;
    }

    private void PostprocessFurrData(List<List<FurrCommand>> furrData)
    {
        foreach (var trigger in furrData)
        {
            foreach (var opcode in trigger)
            {
                switch (opcode.FunctionName)
                {
                    case "CHANGE_POSITION_X": opcode.FunctionName = "CHANGE_POSITION_Z"; break;
                    case "CHANGE_POSITION_Z": opcode.FunctionName = "CHANGE_POSITION_X"; break;
                    case "MOVE_ITEM_X":       opcode.FunctionName = "MOVE_ITEM_Z";        break;
                    case "MOVE_ITEM_Z":       opcode.FunctionName = "MOVE_ITEM_X";        break;
                    case "ADD_POSITION":
                        (opcode.FirstArg, opcode.SecondArg) = (opcode.SecondArg, opcode.FirstArg);
                        break;
                }
            }
        }
    }
}