using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Ai.Configuration;
using TombLauncher.Configuration;

namespace TombLauncher.Extensions;

public static class ConfigurationServiceCollectionExtensions
{
    public static IServiceCollection AddAppConfiguration(
        this IServiceCollection services,
        string appDataDirectory)
    {
        var appConfiguration = new LayeredAppConfiguration();

        IConfiguration defaults = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();
        defaults.Bind(appConfiguration.Defaults);

        var userConfigPath = Path.Combine(appDataDirectory, "appsettings.user.json");
        IConfiguration userConfig = new ConfigurationBuilder()
            .AddJsonFile(userConfigPath, optional: true)
            .Build();
        userConfig.Bind(appConfiguration.User);

        services.AddSingleton<ILayeredAppConfiguration>(appConfiguration);
        services.AddSingleton<IAppConfiguration>(sp => sp.GetRequiredService<ILayeredAppConfiguration>());
        services.AddSingleton<IAiConfig>(sp => sp.GetRequiredService<ILayeredAppConfiguration>().Ai);

        return services;
    }
}
