using TombLauncher.Core.Utils;

namespace TombLauncher.Tests;

public class Md5UtilsTests
{
    private const string EmptyArrayHash = "d41d8cd98f00b204e9800998ecf8427e";
    [Fact]
    public void ComputeMd5Hash_EmptyArrayReturnsKnownMd5()
    {
        var data = Array.Empty<byte>();
        var md5 = Md5Utils.ComputeMd5Hash(data);
        
        Assert.Equal(EmptyArrayHash, md5);
    }

    [Fact]
    public void ComputeMd5Hash_IsOutputLowerCase()
    {
        const string rgx = @"^[0-9a-f]{32}$";
        var data = Array.Empty<byte>();
        var md5 = Md5Utils.ComputeMd5Hash(data);
        
        Assert.Matches(rgx, md5);
    }

    [Fact]
    public async Task ComputeMd5Hash_EmptyStreamReturnsKnownMd5()
    {
        var data = new MemoryStream();
        var md5 = await Md5Utils.ComputeMd5Hash(data);
        
        Assert.Equal(EmptyArrayHash, md5);
    }

    [Fact]
    public async Task ComputeMd5Hash_AreVariantsConsistent()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var fromMemoryStream = await Md5Utils.ComputeMd5Hash(new MemoryStream(bytes));
        var fromArray = Md5Utils.ComputeMd5Hash(bytes);
        
        Assert.Equal(fromArray, fromMemoryStream);
    }
}