using TombLauncher.Core.PlatformSpecific;

namespace TombLauncher.Tests;

public class WindowsPlatformFeaturesTests
{
    private readonly WindowsPlatformSpecificFeatures _sut = new();

    [Theory]
    [InlineData(@"C:\Users\user\games")]
    [InlineData(@"~/games/tr1")]
    [InlineData(@"/opt/games")]
    public void ExpandPath_ReturnsPathUnchanged(string path)
    {
        Assert.Equal(path, _sut.ExpandPath(path));
    }
}
