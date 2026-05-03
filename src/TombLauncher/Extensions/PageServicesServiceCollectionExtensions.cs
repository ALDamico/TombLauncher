using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Services;

namespace TombLauncher.Extensions;

public static class PageServicesServiceCollectionExtensions
{
    public static IServiceCollection AddPageServices(this IServiceCollection services)
    {
        services.AddSingleton<ViewServiceContext>();
        services.AddScoped<GameDetailsService>();
        services.AddScoped<LaunchOptionsService>();
        services.AddScoped<NewGameService>();
        services.AddScoped<GameListService>();
        services.AddScoped<GameWithStatsService>();
        services.AddScoped<AppCrashHostService>();
        services.AddSingleton<WelcomePageService>();
        services.AddTransient<GameSearchService>();
        services.AddTransient<GameSearchResultService>();
        services.AddSingleton<ISettingsProvider, SettingsProvider>();
        services.AddSingleton<SettingsPageService>();
        services.AddScoped<StatisticsService>();
        return services;
    }
}
