using System;
using TombLauncher.Factories;

namespace TombLauncher.Tests;

#nullable enable

public class TestAutomapperConfiguration
{
    [Fact]
    public void ExecuteTest()
    {
        Exception? thrownException = null;
        try
        {
            var configuration = MapperConfigurationFactory.GetMapperConfiguration(Activator.CreateInstance);
            configuration.AssertConfigurationIsValid();
        }
        catch (Exception e)
        {
            thrownException = e;
        }

        Assert.Null(thrownException);
    }
}