using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Services;

namespace TombLauncher.Extensions;

public static class PageServicesServiceCollectionExtensions
{
    public static IServiceCollection AddPageServices(this IServiceCollection services)
    {
        return services.AddSingleton<ViewServiceContext>()
            .AddScoped<GameDetailsService>()
            .AddScoped<LaunchOptionsService>()
            .AddScoped<NewGameService>()
            .AddScoped<GameListService>()
            .AddScoped<GameWithStatsService>()
            .AddScoped<AppCrashHostService>()
            .AddSingleton<WelcomePageService>()
            .AddTransient<GameSearchService>()
            .AddTransient<GameSearchResultService>()
            .AddSingleton<ISettingsProvider, SettingsProvider>()
            .AddSingleton<SettingsPageService>()
            .AddScoped<StatisticsService>()
            .AddSingleton(sp => new NavigationManager(sp));
    }
}
