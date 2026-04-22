using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Services;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Extensions;

public static class ViewModelsServiceCollectionExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
            new WelcomePageViewModel(sp.GetRequiredService<WelcomePageService>()));
        services.AddScoped<GameListViewModel>();
        services.AddScoped<GameSearchViewModel>();
        services.AddTransient<NewGameViewModel>();
        services.AddSingleton<SettingsPageViewModel>();
        services.AddSingleton<NotificationListViewModel>();
        services.AddScoped<StatisticsPageViewModel>();
        services.AddTransient<SavegameListViewModel>();
        services.AddTransient<GameDetailsViewModel>();
        services.AddTransient<AiChatViewModel>();
        return services;
    }
}
