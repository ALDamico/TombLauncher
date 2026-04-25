using System.IO.Compression;
using TombLauncher.Core.Exceptions;
using TombLauncher.Core.Savegames.HeaderReaders;

namespace TombLauncher.Tests;

public class Tr1xSavegameHeaderReaderTests : IDisposable
{
    private readonly string _tempDir;
    private readonly Tr1xSavegameHeaderReader _reader = new();

    public Tr1xSavegameHeaderReaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"Tr1xSavegameHeaderReaderTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    // BSON encoding of {"level_title":"Cave","save_counter":5}
    private static readonly byte[] CaveBson =
    [
        0x2D, 0x00, 0x00, 0x00,                                              // doc size = 45
        0x02,                                                                  // type: string
        0x6C, 0x65, 0x76, 0x65, 0x6C, 0x5F, 0x74, 0x69, 0x74, 0x6C, 0x65, 0x00, // "level_title\0"
        0x05, 0x00, 0x00, 0x00,                                              // string length = 5
        0x43, 0x61, 0x76, 0x65, 0x00,                                        // "Cave\0"
        0x10,                                                                  // type: int32
        0x73, 0x61, 0x76, 0x65, 0x5F, 0x63, 0x6F, 0x75, 0x6E, 0x74, 0x65, 0x72, 0x00, // "save_counter\0"
        0x05, 0x00, 0x00, 0x00,                                              // value = 5
        0x00                                                                   // terminator
    ];

    private static byte[] BuildCompressedBsonBuffer(byte[] uncompressedBson)
    {
        using var compressedMs = new MemoryStream();
        using (var zlib = new ZLibStream(compressedMs, CompressionLevel.Optimal))
            zlib.Write(uncompressedBson);
        var compressed = compressedMs.ToArray();

        var buf = new byte[16 + compressed.Length];
        BitConverter.TryWriteBytes(buf.AsSpan(0, 4), 0x424D3154); // magic
        BitConverter.TryWriteBytes(buf.AsSpan(4, 2), (short)1);
        BitConverter.TryWriteBytes(buf.AsSpan(6, 2), (ushort)1);
        BitConverter.TryWriteBytes(buf.AsSpan(8, 4), compressed.Length);
        BitConverter.TryWriteBytes(buf.AsSpan(12, 4), uncompressedBson.Length);
        compressed.CopyTo(buf, 16);
        return buf;
    }

    [Fact]
    public void ReadHeader_CompressedBson_ExtractsLevelTitle()
    {
        var buf = BuildCompressedBsonBuffer(CaveBson);
        var header = _reader.ReadHeader("savegame_0", buf);
        Assert.Equal("Cave", header.LevelName);
    }

    [Fact]
    public void ReadHeader_CompressedBson_ExtractsSaveCounter()
    {
        var buf = BuildCompressedBsonBuffer(CaveBson);
        var header = _reader.ReadHeader("savegame_0", buf);
        Assert.Equal(5, header.SaveNumber);
    }

    [Fact]
    public void ReadHeader_InvalidData_ThrowsSavegameParseException()
    {
        var garbage = new byte[100];
        new Random(42).NextBytes(garbage);
        Assert.Throws<SavegameParseException>(() => _reader.ReadHeader("savegame_0", garbage));
    }

    [Fact]
    public void ReadHeader_FromFile_ExtractsCorrectData()
    {
        var buf = BuildCompressedBsonBuffer(CaveBson);
        var path = Path.Combine(_tempDir, "savegame_2");
        File.WriteAllBytes(path, buf);
        var header = _reader.ReadHeader(path);
        Assert.Equal("Cave", header.LevelName);
        Assert.Equal(5, header.SaveNumber);
    }
}
