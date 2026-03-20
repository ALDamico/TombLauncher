using TombLauncher.Factories;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using NSubstitute;

namespace TombLauncher.Tests;

public class TestAutomapperConfiguration
{
    [Fact]
    public void ExecuteTest()
    {
        Exception? thrownException = null;
        try
        {
            var configuration = MapperConfigurationFactory.GetMapperConfiguration(Substitute.For<ILoggerFactory>(), t => Activator.CreateInstance(t)!);
            configuration.AssertConfigurationIsValid();
        }
        catch (Exception e)
        {
            thrownException = e;
        }

        Assert.Null(thrownException);
    }
}