using TombLauncher.Contracts.Enums;
using TombLauncher.Installers;
using TombLauncher.Patchers.Widescreen;

namespace TombLauncher.Tests;

public class WidescreenPatcherTests
{
    private static readonly WidescreenPatcher Patcher = new() { EngineDetector = new TombRaiderEngineDetector() };

    public WidescreenPatcherTests()
    {
        if (File.Exists("Data/tomb4.exe.bak"))
        {
            File.Copy("Data/tomb4.exe.bak", "Data/tomb4.exe", true);
            File.Delete("Data/tomb4.exe.bak");
        }

        File.Copy("Data/tomb4.exe", "Data/tomb4.exe.bak", true);
    }

    [Fact]
    public async Task TestWidescreenAspectRatioPatcherDetectChanges()
    {
        var changesToMake = await Patcher.DetectChanges("Data");
        Assert.NotEmpty(changesToMake.AffectedFiles);
    }

    [Fact]
    public async Task TestWidescreenAspectRatioPatcher()
    {
        var parameters = new WidescreenPatcherParameters()
        {
            TargetFolder = "Data",
            TargetAspectRatio = (float)16 / 9,
            UpdateAspectRatio = true
        };
        var result = await Patcher.ApplyPatch("Data", parameters);
        Assert.True(result.IsSuccessful);
        Assert.Equal(0xa73d0, result.AffectedFiles.FirstOrDefault()?.Offset);
    }

    [Fact]
    public async Task TestCameraDistancePatch()
    {
        var parameters = new WidescreenPatcherParameters()
        {
            TargetFolder = "Data",
            UpdateCameraDistance = true,
            TargetCameraDistance = 2048
        };
        var result = await Patcher.ApplyPatch("Data", parameters);
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.AffectedFiles.FirstOrDefault()?.Offset);
    }

    [Fact]
    public async Task TestFovPatch()
    {
        var parameters = new WidescreenPatcherParameters()
        {
            TargetFolder = "Data",
            UpdateFov = true,
            TargetFov = 1920
        };
        var result = await Patcher.ApplyPatch("Data", parameters);
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.AffectedFiles.FirstOrDefault()?.Offset);
    }

    [Fact]
    public async Task Test60FpsNotAppliedForTr4()
    {
        var originalBytes = await File.ReadAllBytesAsync("Data/tomb4.exe");
        var parameters = new WidescreenPatcherParameters()
        {
            TargetFolder = "Data",
            Update60Fps = true,
            Engine = GameEngine.TombRaider4
        };
        var result = await Patcher.ApplyPatch("Data", parameters);
        var patchedBytes = await File.ReadAllBytesAsync("Data/tomb4.exe");
        Assert.True(result.IsSuccessful);
        Assert.Equal(originalBytes, patchedBytes);
    }
}