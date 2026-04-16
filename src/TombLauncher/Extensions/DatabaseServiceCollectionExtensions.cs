using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Configuration;
using TombLauncher.Data.Database;
using TombLauncher.Data.Database.Repositories;
using TombLauncher.Data.Database.Services;

namespace TombLauncher.Extensions;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseAccess(this IServiceCollection services,
        IAppConfiguration appConfiguration, string appDataDirectory)
    {
        services.AddDbContext<TombLauncherDbContext>(opts =>
        {
            var databasePath = Path.Combine(appDataDirectory,
                appConfiguration.Application.DatabasePath ?? "TombLauncher.sqlite");
            var connectionString = $"Data Source={databasePath}";
            opts.UseSqlite(connectionString);
        });
        services.AddScoped<ISavegameRepository, SavegameRepository>();
        services.AddScoped<GameDataService>();
        services.AddScoped<PlaySessionDataService>();
        services.AddScoped<GameLinkDataService>();
        services.AddScoped<GameHashDataService>();
        services.AddScoped<StatisticsDataService>();
        services.AddScoped<AppCrashDataService>();
        return services;
    }
}
