using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TombLauncher.Data.Mapping;
using TombLauncher.Factories;
using TombLauncher.Factories.Mapping;

namespace TombLauncher.Extensions;

public static class MappingServiceCollectionExtensions
{
    public static IServiceCollection AddTombLauncherMappings(this IServiceCollection services)
    {
        services.AddSingleton(sp => MapperConfigurationFactory.GetMapperConfiguration(sp.GetService<ILoggerFactory>()!, t => sp.GetService(t)!));
        services.AddSingleton<IMapper>(sp =>
        {
            var config = sp.GetRequiredService<MapperConfiguration>();
            return config.CreateMapper(t => sp.GetService(t)!);
        });
        services.AddSingleton<SettingsMapper>();
        services.AddSingleton<AppCrashMapper>();
        services.AddSingleton<StatisticsMapper>();
        return services;
    }
}
