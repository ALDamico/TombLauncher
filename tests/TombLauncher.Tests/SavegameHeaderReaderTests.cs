using TombLauncher.Core.Exceptions;
using TombLauncher.Core.Savegames.HeaderReaders;

namespace TombLauncher.Tests;

public class SavegameHeaderReaderTests : IDisposable
{
    private readonly string _tempDir;
    private readonly SavegameHeaderReader _reader = new();

    public SavegameHeaderReaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"SavegameHeaderReaderTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private static byte[] BuildBuffer(string levelName, int saveNumber)
    {
        var buf = new byte[80];
        var nameBytes = System.Text.Encoding.ASCII.GetBytes(levelName);
        Array.Copy(nameBytes, buf, Math.Min(nameBytes.Length, 74));
        BitConverter.TryWriteBytes(buf.AsSpan(75, 4), saveNumber);
        return buf;
    }

    [Fact]
    public void ReadHeader_ExtractsLevelName()
    {
        var buf = BuildBuffer("Caves", 1);
        var header = _reader.ReadHeader("savegame.0", buf);
        Assert.Equal("Caves", header.LevelName);
    }

    [Fact]
    public void ReadHeader_ExtractsSaveNumber()
    {
        var buf = BuildBuffer("Caves", 42);
        var header = _reader.ReadHeader("savegame.0", buf);
        Assert.Equal(42, header.SaveNumber);
    }

    [Fact]
    public void ReadHeader_ReplacesAsterisksWithSpaces()
    {
        var buf = BuildBuffer("Lara's*Home", 1);
        var header = _reader.ReadHeader("savegame.1", buf);
        Assert.Equal("Lara's Home", header.LevelName);
    }

    [Fact]
    public void ReadHeader_ExtractsSlotNumberFromFilename()
    {
        var buf = BuildBuffer("Caves", 1);
        var header = _reader.ReadHeader("savegame.5", buf);
        Assert.Equal(6, header.SlotNumber);
    }

    [Fact]
    public void ReadHeader_BufferTooSmall_Throws()
    {
        Assert.Throws<SavegameParseException>(() => _reader.ReadHeader("savegame.0", new byte[79]));
    }

    [Fact]
    public void ReadHeader_FromFile_ExtractsCorrectData()
    {
        var buf = BuildBuffer("Athens", 7);
        var path = Path.Combine(_tempDir, "savegame.3");
        File.WriteAllBytes(path, [..buf, ..new byte[20]]);
        var header = _reader.ReadHeader(path);
        Assert.Equal("Athens", header.LevelName);
        Assert.Equal(7, header.SaveNumber);
        Assert.Equal(4, header.SlotNumber);
    }

    [Fact]
    public void ReadHeader_FileTooShort_Throws()
    {
        var path = Path.Combine(_tempDir, "savegame.0");
        File.WriteAllBytes(path, new byte[50]);
        Assert.Throws<SavegameParseException>(() => _reader.ReadHeader(path));
    }
}
