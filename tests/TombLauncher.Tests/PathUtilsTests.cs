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
        var chars = PathUtils.GetWindowsInvalidFileNameChars();
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

    [Fact]
    public void GetDirectorySize_DoesNotFollowSymlinks()
    {
        var folderA = Path.Combine(_tempDir, "folderA");
        var folderB = Path.Combine(_tempDir, "folderB");
        Directory.CreateDirectory(folderA);
        Directory.CreateDirectory(folderB);

        File.WriteAllText(Path.Combine(folderA, "a.txt"), "A"); // 1 byte
        File.WriteAllBytes(Path.Combine(folderB, "big.bin"), new byte[1000]); // 1000 byte

        // symlink dentro folderA che punta a folderB
        Directory.CreateSymbolicLink(Path.Combine(folderA, "linkToB"), folderB);

        var size = PathUtils.GetDirectorySize(folderA);
        Assert.Equal(1L, size); // solo a.txt, non big.bin
    }

    [Fact]
    public void GetDirectorySize_SkipsProtonPrefixDirectory()
    {
        var folder = Path.Combine(_tempDir, "proton");
        var protonPfxFolder = Path.Combine(folder, "proton_pfx");
        Directory.CreateDirectory(folder);
        Directory.CreateDirectory(protonPfxFolder);
        var arr = new byte[1000];
        WriteEmptyArray(folder, arr);
        WriteEmptyArray(protonPfxFolder, arr);

        var size = PathUtils.GetDirectorySize(folder);
        Assert.Equal(1000, size); // only "proton/a.bin", skips "proton_pfx/a.bin"
    }

    [Fact]
    public void GetDirectorySize_DoesNotThrow_ForInaccessibleSubdirectory()
    {
        if (OperatingSystem.IsWindows())
            return;
        var dir = Path.Combine(_tempDir, "accessible");
        Directory.CreateDirectory(dir);
        var inaccessibleDir = Path.Combine(dir, "inaccessible");
        Directory.CreateDirectory(inaccessibleDir);
        var arr = new byte[500];
        WriteEmptyArray(dir, arr);
        WriteEmptyArray(inaccessibleDir, arr);

        long directorySize = -1;
        Exception? exception = null;
        try
        {
            File.SetUnixFileMode(inaccessibleDir, UnixFileMode.None);
            directorySize = PathUtils.GetDirectorySize(dir);
        }
        catch(Exception ex)
        {
            exception = ex;
        }
        finally
        {
            File.SetUnixFileMode(inaccessibleDir, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
        
        
        Assert.Equal(500, directorySize);
        Assert.Null(exception);
    }

    private static void WriteEmptyArray(string folder, byte[] array)
    {
        var fileName = Path.Combine(folder, "a.bin");
        File.WriteAllBytes(fileName, array);
    }
}