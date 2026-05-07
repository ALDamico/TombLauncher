using TombLauncher.Contracts;
using TombLauncher.Installers;
using TombLauncher.Patchers.Widescreen;

namespace TombLauncher.Tests;

public class WidescreenPatcherTests
{
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
        var patcher = new WidescreenPatcher()
        {
            EngineDetector = new TombRaiderEngineDetector()
        };
        var changesToMake = await patcher.DetectChanges("Data");
        Assert.NotEmpty(changesToMake.AffectedFiles);
    }

    [Fact]
    public async Task TestWidescreenAspectRatioPatcher()
    {
        var patcher = new WidescreenPatcher()
        {
            EngineDetector = new TombRaiderEngineDetector() 
        };
        var changesToMake = await patcher.DetectChanges("Data");
        var parameters = new WidescreenPatcherParameters()
        {
            TargetFolder = "Data",
            OriginalAspectRatio = (float)4 / 3,
            TargetAspectRatio = (float)16 / 9,
            UpdateAspectRatio = true
        };
        var successful = await patcher.ApplyPatch("Data", parameters);
        Assert.True(successful.IsSuccessful);
        Assert.True(successful.AffectedFiles.FirstOrDefault()?.Offset == 0xa73d0);
    }
}