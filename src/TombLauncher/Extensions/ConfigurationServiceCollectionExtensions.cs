using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Ai.Configuration;
using TombLauncher.Configuration;

namespace TombLauncher.Extensions;

public static class ConfigurationServiceCollectionExtensions
{
    public static IServiceCollection AddTombLauncherConfiguration(this IServiceCollection serviceCollection, ILayeredAppConfiguration appConfiguration, string appDataDirectory)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();
        configuration.Bind(appConfiguration.Defaults);
        var userConfigPath = Path.Combine(appDataDirectory, "appsettings.user.json");
        IConfiguration userConfiguration = new ConfigurationBuilder()
            .AddJsonFile(userConfigPath, optional: true)
            .Build();
        userConfiguration.Bind(appConfiguration.User);
        return serviceCollection.AddSingleton(appConfiguration)
            .AddSingleton<IAppConfiguration>(sp => sp.GetRequiredService<ILayeredAppConfiguration>())
            .AddSingleton<IAiConfig>(sp => sp.GetRequiredService<ILayeredAppConfiguration>().Ai);
    }
}