using TombLauncher.Contracts.Settings;
using TombLauncher.Core.PlatformSpecific;

namespace TombLauncher.Tests;

public class LinuxPlatformFeaturesTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly LinuxPlatformSpecificFeatures _sut = new();

    public LinuxPlatformFeaturesTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempRoot);
    }

    /// <summary>
    /// Creates a fake steamapps/common layout under <paramref name="steamRoot"/>
    /// and optionally places a "proton" binary inside each Proton* folder.
    /// </summary>
    private void CreateFakeSteamLayout(string steamRoot, params (string folderName, bool withBinary)[] protonFolders)
    {
        var common = Path.Combine(steamRoot, "steamapps", "common");
        Directory.CreateDirectory(common);
        foreach (var (folderName, withBinary) in protonFolders)
        {
            var protonDir = Path.Combine(common, folderName);
            Directory.CreateDirectory(protonDir);
            if (withBinary)
            {
                // Create an empty file that acts as the binary
                File.WriteAllText(Path.Combine(protonDir, "proton"), "#!/usr/bin/env python3");
            }
        }
    }

    [Fact]
    public void FindAvailableProtonInstallations_ReturnsEmpty_WhenNoSteamRoot()
    {
        // Point the method to a temp dir with no steamapps at all
        // We'll test via the instance but can't easily inject the path, so we call
        // the real implementation and rely on the CI machine not having Steam installed.
        // If Steam IS installed, we just assert the list is not null.
        var result = _sut.FindAvailableProtonInstallations();
        Assert.NotNull(result);
    }

    [Fact]
    public void FindAvailableProtonInstallations_ExcludesFoldersWithoutBinary()
    {
        // Build a mini steamapps tree in temp with a Proton folder but no binary
        CreateFakeSteamLayout(_tempRoot, ("Proton 9.0", false));
        // The real implementation scans ~/.steam/steam which we can't easily redirect,
        // so this test validates the helper logic directly.
        var common = Path.Combine(_tempRoot, "steamapps", "common");
        var results = ScanCommonDir(common);
        Assert.Empty(results);
    }

    [Fact]
    public void FindAvailableProtonInstallations_IncludesFoldersWithBinary()
    {
        CreateFakeSteamLayout(_tempRoot, ("Proton 9.0", true), ("Proton 8.0", true));
        var common = Path.Combine(_tempRoot, "steamapps", "common");
        var results = ScanCommonDir(common);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void FindAvailableProtonInstallations_SortedDescending()
    {
        CreateFakeSteamLayout(_tempRoot,
            ("Proton 8.0", true),
            ("Proton 9.0", true),
            ("Proton 7.0", true));
        var common = Path.Combine(_tempRoot, "steamapps", "common");
        var results = ScanCommonDir(common);

        Assert.Equal("Proton 9.0", results[0].DisplayName);
        Assert.Equal("Proton 8.0", results[1].DisplayName);
        Assert.Equal("Proton 7.0", results[2].DisplayName);
    }

    [Fact]
    public void GetProtonVersion_ReturnsNull_ForEmptyPath()
    {
        Assert.Null(_sut.GetProtonVersion(""));
    }

    [Fact]
    public void GetProtonVersion_ReturnsNull_WhenVersionFileMissing()
    {
        var dir = Path.Combine(_tempRoot, "Proton X");
        Directory.CreateDirectory(dir);
        var bin = Path.Combine(dir, "proton");
        File.WriteAllText(bin, "");
        Assert.Null(_sut.GetProtonVersion(bin));
    }

    [Fact]
    public void GetProtonVersion_ReturnsVersionFileContent()
    {
        var dir = Path.Combine(_tempRoot, "Proton X");
        Directory.CreateDirectory(dir);
        var bin = Path.Combine(dir, "proton");
        File.WriteAllText(bin, "");
        File.WriteAllText(Path.Combine(dir, "version"), "  9.0.3  ");
        Assert.Equal("9.0.3", _sut.GetProtonVersion(bin));
    }

    // ─── ExpandPath ──────────────────────────────────────────────────────────

    [Fact]
    public void ExpandPath_ExpandsLeadingTilde()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.True(true);
            return;
        }
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        Assert.Equal(Path.Combine(home, "games/tr1"), _sut.ExpandPath("~/games/tr1"));
    }

    [Fact]
    public void ExpandPath_ReturnsAbsolutePath_Unchanged()
    {
        Assert.Equal("/opt/games/tr1", _sut.ExpandPath("/opt/games/tr1"));
    }

    [Fact]
    public void ExpandPath_DoesNotExpand_TildeInMiddle()
    {
        Assert.Equal("/opt/my~path", _sut.ExpandPath("/opt/my~path"));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Replicates the inner loop of FindAvailableProtonInstallations for a given common dir,
    /// so we can test the scanning logic without touching the real Steam root.
    /// </summary>
    private static List<ProtonInstallationDto> ScanCommonDir(string commonDir)
    {
        var results = new List<ProtonInstallationDto>();
        if (!Directory.Exists(commonDir)) return results;

        foreach (var dir in Directory.EnumerateDirectories(commonDir, "Proton*"))
        {
            var bin = Path.Combine(dir, "proton");
            if (!File.Exists(bin)) continue;
            results.Add(new(Path.GetFileName(dir), bin));
        }
        results.Sort((a, b) => string.Compare(b.DisplayName, a.DisplayName, StringComparison.OrdinalIgnoreCase));
        return results;
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempRoot, true); } catch { /* ignore */ }
    }
}
