using TombLauncher.Core.Utils;

namespace TombLauncher.Tests;

public class PathUtilsTests : IDisposable
{
    private readonly string _tempDir;

    public PathUtilsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"PathUtilsTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    // --- GetDirectorySize ---

    [Fact]
    public void GetDirectorySize_ReturnsZero_ForEmptyDirectory()
    {
        var emptyDir = Path.Combine(_tempDir, "empty");
        Directory.CreateDirectory(emptyDir);

        Assert.Equal(0L, PathUtils.GetDirectorySize(emptyDir));
    }

    [Fact]
    public void GetDirectorySize_ReturnsSumOfFileSizes()
    {
        var dir = Path.Combine(_tempDir, "sized");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "a.txt"), "hello"); // 5 bytes
        File.WriteAllText(Path.Combine(dir, "b.txt"), "world!"); // 6 bytes

        var result = PathUtils.GetDirectorySize(dir);
        Assert.Equal(11L, result);
    }

    [Fact]
    public void GetDirectorySize_IncludesSubdirectoryFiles()
    {
        var dir = Path.Combine(_tempDir, "nested");
        var subDir = Path.Combine(dir, "sub");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(dir, "root.txt"), "abc"); // 3
        File.WriteAllText(Path.Combine(subDir, "deep.txt"), "de"); // 2

        Assert.Equal(5L, PathUtils.GetDirectorySize(dir));
    }

    // --- DirectoryContainsFile ---

    [Fact]
    public void DirectoryContainsFile_ReturnsTrue_WhenFileExists()
    {
        File.WriteAllText(Path.Combine(_tempDir, "target.dat"), "data");
        Assert.True(PathUtils.DirectoryContainsFile(_tempDir, "target.dat"));
    }

    [Fact]
    public void DirectoryContainsFile_ReturnsFalse_WhenFileDoesNotExist()
    {
        Assert.False(PathUtils.DirectoryContainsFile(_tempDir, "missing.dat"));
    }

    [Fact]
    public void DirectoryContainsFile_FindsFileInSubdirectory()
    {
        var subDir = Path.Combine(_tempDir, "subdir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "nested.txt"), "content");

        Assert.True(PathUtils.DirectoryContainsFile(_tempDir, "nested.txt"));
    }

    // --- GetRelativePath ---

    [Fact]
    public void GetRelativePath_ReturnsRelativePath_WhenFileExists()
    {
        var subDir = Path.Combine(_tempDir, "folder");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "readme.txt"), "hi");

        var result = PathUtils.GetRelativePath(_tempDir, "readme.txt");
        Assert.Equal(Path.Combine("folder", "readme.txt"), result);
    }

    [Fact]
    public void GetRelativePath_ReturnsNull_WhenFileDoesNotExist()
    {
        var result = PathUtils.GetRelativePath(_tempDir, "nonexistent.txt");
        Assert.Null(result);
    }

    // --- GetWindowsInvalidFileNameChars ---

    [Fact]
    public void GetWindowsInvalidFileNameChars_WithoutSeparators_DoesNotContainSlashes()
    {
        var chars = PathUtils.GetWindowsInvalidFileNameChars(false);
        Assert.DoesNotContain('\\', chars);
        Assert.DoesNotContain('/', chars);
    }

    [Fact]
    public void GetWindowsInvalidFileNameChars_WithSeparators_ContainsSlashes()
    {
        var chars = PathUtils.GetWindowsInvalidFileNameChars(true);
        Assert.Contains('\\', chars);
        Assert.Contains('/', chars);
    }

    // --- GetTombLauncherTempDirectory ---

    [Fact]
    public void GetTombLauncherTempDirectory_ReturnsPathUnderSystemTemp()
    {
        var result = PathUtils.GetTombLauncherTempDirectory();
        Assert.StartsWith(Path.GetTempPath(), result);
        Assert.EndsWith("TombLauncher", result);
        Assert.True(Directory.Exists(result));
    }
}
