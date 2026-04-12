using TombLauncher.Core.HardwareInfo;

namespace TombLauncher.Tests;

public class HardwareInfoServiceTests
{
    [Fact]
    public async Task TestHardwareDetection()
    {
        var hwInfoService = new HardwareInfoService();
        var hwInfo = await hwInfoService.DetectHardware();
        Assert.True(hwInfo.TotalRamMb > 0);
    }
}