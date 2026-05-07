using Microsoft.Extensions.DependencyInjection;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Extensions;

public static class ViewModelsServiceCollectionExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        return services.AddSingleton<WelcomePageViewModel>()
            .AddScoped<GameListViewModel>()
            .AddScoped<GameSearchViewModel>()
            .AddTransient<NewGameViewModel>()
            .AddSingleton<SettingsPageViewModel>()
            .AddSingleton<NotificationListViewModel>()
            .AddScoped<StatisticsPageViewModel>()
            .AddTransient<SavegameListViewModel>()
            .AddTransient<GameDetailsViewModel>()
            .AddTransient<LaunchOptionsViewModel>()
            .AddSingleton<MainWindowViewModel>()
            .AddTransient<LaunchOptionsViewModel>()
            .AddSingleton<AboutPageViewModel>()
            .AddTransient<AiChatViewModel>();
    }
}
