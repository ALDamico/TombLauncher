using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Data.Mapping;
using TombLauncher.Mappers;
using TombLauncher.Mapping;

namespace TombLauncher.Extensions;

public static class MappingServiceCollectionExtensions
{
    public static IServiceCollection AddTombLauncherMappings(this IServiceCollection services)
    {
        services.AddSingleton<SettingsMapper>()
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
            .AddSingleton<SearchMapper>()
            .AddSingleton<SavegameMapper>();
        return services;
    }
}