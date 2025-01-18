using TombLauncher.Contracts;
using TombLauncher.Patchers.Widescreen;

namespace TombLauncher.Tests;

public class TestWidescreenPatcher
{

    public TestWidescreenPatcher()
    {
        if (File.Exists("TestAssets/tomb4.exe.bak"))
        {
            File.Copy("TestAssets/tomb4.exe.bak", "TestAssets/tomb4.exe", true);
            File.Delete("TestAssets/tomb4.exe.bak");
        }
        
        File.Copy("TestAssets/tomb4.exe", "TestAssets/tomb4.exe.bak", true);
    }
    
    [Fact]
    public async Task TestWidescreenAspectRatioPatcherDetectChanges()
    {
        var patcher = new WidescreenPatcher();
        patcher.EngineDetector = new TombRaiderEngineDetector();
        var changesToMake = await patcher.DetectChanges("TestAssets");
        Assert.NotEmpty(changesToMake.AffectedFiles);
    }

    [Fact]
    public async Task TestWidescreenAspectRatioPatcher()
    {
        var patcher = new WidescreenPatcher();
        patcher.EngineDetector = new TombRaiderEngineDetector();
        var changesToMake = await patcher.DetectChanges("TestAssets");
        var parameters = new WidescreenPatcherParameters()
        {
            TargetFolder = "TestAssets",
            OriginalAspectRatio = (float)4 / 3,
            TargetAspectRatio = (float)16 / 9,
            UpdateAspectRatio = true
        };
        var successful = await patcher.ApplyPatch("TestAssets", parameters);
        Assert.True(successful.IsSuccessful);
        Assert.True(successful.AffectedFiles.FirstOrDefault()?.StartOffset == 0xa73d0);
    }
}