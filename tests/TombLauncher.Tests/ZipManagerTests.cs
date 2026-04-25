using System.IO.Compression;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Utils;

namespace TombLauncher.Tests;

public class ZipManagerTests : IDisposable
{
    private readonly string _tempDir;

    public ZipManagerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ZipManagerTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string CreateTestZip(params (string name, string content)[] entries)
    {
        var zipPath = Path.Combine(_tempDir, $"test_{Guid.NewGuid():N}.zip");
        using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        foreach (var (name, content) in entries)
        {
            var entry = archive.CreateEntry(name);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(content);
        }
        return zipPath;
    }

    [Fact]
    public void GetEntries_ReturnsAllEntries()
    {
        var zipPath = CreateTestZip(("a.txt", "hello"), ("b.txt", "world"));
        using var manager = new ZipManager(zipPath);
        Assert.Equal(2, manager.GetEntries().Count());
    }

    [Fact]
    public async Task ExtractAll_ExtractsFilesWithCorrectContent()
    {
        var zipPath = CreateTestZip(("file.txt", "hello world"));
        var outDir = Path.Combine(_tempDir, "out");
        using var manager = new ZipManager(zipPath);
        await manager.ExtractAll(outDir);
        Assert.True(File.Exists(Path.Combine(outDir, "file.txt")));
        Assert.Equal("hello world", await File.ReadAllTextAsync(Path.Combine(outDir, "file.txt")));
    }

    [Fact]
    public async Task ExtractAll_PreservesDirectoryStructure()
    {
        var zipPath = CreateTestZip(("sub/nested.txt", "content"));
        var outDir = Path.Combine(_tempDir, "out");
        using var manager = new ZipManager(zipPath);
        await manager.ExtractAll(outDir);
        Assert.True(File.Exists(Path.Combine(outDir, "sub", "nested.txt")));
    }

    [Fact]
    public async Task ExtractAll_ReportsProgress()
    {
        var zipPath = CreateTestZip(("a.txt", "a"), ("b.txt", "b"));
        var outDir = Path.Combine(_tempDir, "out");
        var reports = new List<CopyProgressInfo>();
        using var manager = new ZipManager(zipPath);
        await manager.ExtractAll(outDir, progress: new Progress<CopyProgressInfo>(reports.Add));
        Assert.NotEmpty(reports);
        Assert.All(reports, r => Assert.Equal(2, r.TotalFiles));
    }

    [Fact]
    public async Task ExtractAll_CancelledToken_Throws()
    {
        var zipPath = CreateTestZip(("file.txt", "content"));
        var outDir = Path.Combine(_tempDir, "out");
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        using var manager = new ZipManager(zipPath);
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => manager.ExtractAll(outDir, cts.Token));
    }
}
