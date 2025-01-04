using TombLauncher.Factories;

namespace TombLauncher.Tests;

public class TestAutomapperConfiguration
{
    [Fact]
    public void ExecuteTest()
    {
        Exception thrownException = null;
        try
        {
            var configuration = MapperConfigurationFactory.GetMapperConfiguration();
            configuration.AssertConfigurationIsValid();
        }
        catch (Exception e)
        {
            thrownException = e;
        }

        Assert.Null(thrownException);
    }
}