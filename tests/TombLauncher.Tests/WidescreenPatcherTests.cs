using Microsoft.Extensions.Logging.Abstractions;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Patchers;
using TombLauncher.Core.Patchers;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Installers;
using TombLauncher.Patchers.Widescreen;

namespace TombLauncher.Tests;

public class WidescreenPatcherTests
{
    private readonly WidescreenPatcher _patcher;

    private readonly ProgressLogger _progress = new(new Progress<LogEntry>(msg => Console.WriteLine(msg.Message)));

    public WidescreenPatcherTests()
    {
        if (File.Exists("Data/tomb4.exe.bak"))
        {
            File.Copy("Data/tomb4.exe.bak", "Data/tomb4.exe", true);
            File.Delete("Data/tomb4.exe.bak");
        }

        File.Copy("Data/tomb4.exe", "Data/tomb4.exe.bak", true);

        IPlatformSpecificFeatures platformSpecificFeatures;
        if (OperatingSystem.IsLinux())
            platformSpecificFeatures = new LinuxPlatformSpecificFeatures();
        else
            platformSpecificFeatures = new WindowsPlatformSpecificFeatures();
        _patcher = new(new TombRaiderEngineDetector(platformSpecificFeatures), NullLogger<WidescreenPatcher>.Instance);
    }

    [Fact]
    public async Task TestWidescreenAspectRatioPatcherDetectChanges()
    {
        var changesToMake = await _patcher.DetectChanges("Data", _progress);
        Assert.NotEmpty(changesToMake.AffectedFiles);
    }

    [Fact]
    public async Task TestWidescreenAspectRatioPatcher()
    {
        var parameters = new WidescreenPatcherParameters()
        {
            TargetAspectRatio = (float)16 / 9,
            UpdateAspectRatio = true
        };
        var result = await _patcher.ApplyPatch("Data", parameters, _progress);
        Assert.True(result.IsSuccessful);
        Assert.Equal(0xa73d0, result.AffectedFiles.FirstOrDefault()?.Offset);
    }

    [Fact]
    public async Task TestCameraDistancePatch()
    {
        var parameters = new WidescreenPatcherParameters()
        {
            UpdateCameraDistance = true,
            TargetCameraDistance = 2048
        };
        var result = await _patcher.ApplyPatch("Data", parameters, _progress);
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.AffectedFiles.FirstOrDefault()?.Offset);
    }

    [Fact]
    public async Task TestFovPatch()
    {
        var parameters = new WidescreenPatcherParameters()
        {
            UpdateFov = true,
            TargetFov = 1920
        };
        var result = await _patcher.ApplyPatch("Data", parameters, _progress);
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.AffectedFiles.FirstOrDefault()?.Offset);
    }

    [Fact]
    public async Task Test60FpsNotAppliedForTr4()
    {
        var originalBytes = await File.ReadAllBytesAsync("Data/tomb4.exe");
        var parameters = new WidescreenPatcherParameters()
        {
            Update60Fps = true,
            Engine = GameEngine.TombRaider4
        };
        var result = await _patcher.ApplyPatch("Data", parameters, _progress);
        var patchedBytes = await File.ReadAllBytesAsync("Data/tomb4.exe");
        Assert.True(result.IsSuccessful);
        Assert.Equal(originalBytes, patchedBytes);
    }
}