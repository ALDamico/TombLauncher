using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TombLauncher.Data.Mapping;
using TombLauncher.Factories;
using TombLauncher.Mappers;
using TombLauncher.Mapping;

namespace TombLauncher.Extensions;

public static class MappingServiceCollectionExtensions
{
    public static IServiceCollection AddTombLauncherMappings(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
                MapperConfigurationFactory.GetMapperConfiguration(sp.GetService<ILoggerFactory>()!,
                    t => sp.GetService(t)!))
            .AddSingleton<IMapper>(sp =>
            {
                var config = sp.GetRequiredService<MapperConfiguration>();
                return config.CreateMapper(t => sp.GetService(t)!);
            })
            .AddSingleton<SettingsMapper>()
            .AddSingleton<AppCrashMapper>()
            .AddSingleton<StatisticsMapper>()
            .AddSingleton<GameLinkMapper>()
            .AddSingleton<GameLinkDtoMapper>()
            .AddSingleton<GameHashMapper>()
            .AddSingleton<FileBackupMapper>()
            .AddSingleton<GameMetadataMapper>()
            .AddSingleton<GameMapper>()
            .AddSingleton<DownloaderSearchPayloadMapper>()
            .AddSingleton<LaunchOptionsMapper>()
            .AddSingleton<SearchMapper>();
        return services;
    }
}