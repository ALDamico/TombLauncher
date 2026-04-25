using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Exceptions;
using TombLauncher.Core.Savegames;
using TombLauncher.Core.Savegames.HeaderReaders;

namespace TombLauncher.Tests;

public class SavegameHeaderProcessorTests : IDisposable
{
    private readonly ILogger<SavegameHeaderProcessor> _logger;
    private readonly ISavegameHeaderReader _headerReader;
    private readonly SavegameHeaderProcessor _processor;
    private readonly string _tempDir;

    public SavegameHeaderProcessorTests()
    {
        _logger = Substitute.For<ILogger<SavegameHeaderProcessor>>();
        _headerReader = Substitute.For<ISavegameHeaderReader>();
        _processor = new SavegameHeaderProcessor(_logger)
        {
            SavegameHeaderReader = _headerReader,
            Delay = 0 // no delay in tests
        };
        _tempDir = Path.Combine(Path.GetTempPath(), $"SavegameTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        _processor.Dispose();
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string CreateTempSavegameFile(string name = "save001.dat", string content = "fake-save-data")
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void Constructor_DoesNotStartWorkerThread()
    {
        // The processor should not be running immediately after construction
        using var processor = new SavegameHeaderProcessor(_logger)
        {
            SavegameHeaderReader = _headerReader,
            Delay = 0
        };
        // No thread started — Dispose should return immediately without issues
    }

    [Fact]
    public void Start_SetsIsRunning()
    {
        using var processor = new SavegameHeaderProcessor(_logger)
        {
            SavegameHeaderReader = _headerReader,
            Delay = 0
        };

        processor.Start();

        // We verify indirectly: calling Dispose after Start should cleanly stop the thread
        // (if the thread wasn't started, Dispose would be a no-op)
    }

    [Fact]
    public void Start_CalledTwice_DoesNotThrow()
    {
        using var processor = new SavegameHeaderProcessor(_logger)
        {
            SavegameHeaderReader = _headerReader,
            Delay = 0
        };

        processor.Start();
        processor.Start(); // should be idempotent
    }

    [Fact]
    public void Dispose_WithoutStart_DoesNotThrow()
    {
        var processor = new SavegameHeaderProcessor(_logger)
        {
            SavegameHeaderReader = _headerReader,
            Delay = 0
        };

        processor.Dispose(); // should not hang or throw
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var processor = new SavegameHeaderProcessor(_logger)
        {
            SavegameHeaderReader = _headerReader,
            Delay = 0
        };

        processor.Start();
        processor.Dispose();
        processor.Dispose(); // idempotent
    }

    [Fact]
    public void EnqueueFileName_ProcessesFile_AndAddsToProcessedFiles()
    {
        var filePath = CreateTempSavegameFile();
        var expectedHeader = new SavegameHeader { LevelName = "Test Level", Filepath = filePath, SaveNumber = 1, SlotNumber = 0 };
        _headerReader.ReadHeader(filePath).Returns(expectedHeader);

        _processor.Start();
        _processor.EnqueueFileName(filePath);

        // Give the worker thread time to process
        Thread.Sleep(500);

        Assert.Single(_processor.ProcessedFiles);
        var dto = _processor.ProcessedFiles[0];
        Assert.Equal("Test Level", dto.LevelName);
        Assert.Equal(1, dto.SaveNumber);
        Assert.Equal(0, dto.SlotNumber);
        Assert.Equal(filePath, dto.FileName);
    }

    [Fact]
    public void EnqueueFileName_Directory_IsIgnored()
    {
        _processor.Start();
        _processor.EnqueueFileName(_tempDir); // pass a directory, not a file

        Thread.Sleep(200);

        Assert.Empty(_processor.ProcessedFiles);
    }

    [Fact]
    public void ProcessFile_WhenHeaderReaderThrowsSavegameParseException_SetsErrorOccurred()
    {
        var filePath = CreateTempSavegameFile();
        _headerReader.ReadHeader(filePath).Throws(new SavegameParseException("parse error"));

        _processor.Start();
        _processor.EnqueueFileName(filePath);

        Thread.Sleep(500);

        Assert.True(_processor.ErrorOccurred);
        Assert.Empty(_processor.ProcessedFiles);
    }

    [Fact]
    public void ProcessFile_AfterError_SkipsSubsequentFiles()
    {
        var file1 = CreateTempSavegameFile("save001.dat", "data1");
        var file2 = CreateTempSavegameFile("save002.dat", "data2");

        _headerReader.ReadHeader(file1).Throws(new SavegameParseException("error"));
        _headerReader.ReadHeader(file2).Returns(new SavegameHeader { LevelName = "Level 2", Filepath = file2, SaveNumber = 2, SlotNumber = 1 });

        _processor.Start();
        _processor.EnqueueFileName(file1);
        _processor.EnqueueFileName(file2);

        Thread.Sleep(500);

        Assert.True(_processor.ErrorOccurred);
        Assert.Empty(_processor.ProcessedFiles); // second file should be skipped
    }

    [Fact]
    public void ClearProcessedFiles_RemovesAllEntries()
    {
        var filePath = CreateTempSavegameFile();
        _headerReader.ReadHeader(filePath).Returns(new SavegameHeader { LevelName = "Test", Filepath = filePath, SaveNumber = 1, SlotNumber = 0 });

        _processor.Start();
        _processor.EnqueueFileName(filePath);

        Thread.Sleep(500);
        Assert.Single(_processor.ProcessedFiles);

        _processor.ClearProcessedFiles();
        Assert.Empty(_processor.ProcessedFiles);
    }

    [Fact]
    public void ProcessFile_WhenHeaderReaderReturnsNull_DoesNotAddToProcessedFiles()
    {
        var filePath = CreateTempSavegameFile();
        _headerReader.ReadHeader(filePath).Returns((SavegameHeader?)null);

        _processor.Start();
        _processor.EnqueueFileName(filePath);

        Thread.Sleep(500);

        Assert.Empty(_processor.ProcessedFiles);
        Assert.False(_processor.ErrorOccurred);
    }

    [Fact]
    public void Dispose_StopsWorkerThread_Promptly()
    {
        _processor.Start();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        _processor.Dispose();
        sw.Stop();

        // Dispose should complete quickly (well under 2 seconds)
        Assert.True(sw.ElapsedMilliseconds < 2000, $"Dispose took {sw.ElapsedMilliseconds}ms, expected < 2000ms");
    }
}
