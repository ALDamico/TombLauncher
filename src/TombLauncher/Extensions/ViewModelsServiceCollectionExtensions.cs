using Microsoft.Extensions.DependencyInjection;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;
using TombLauncher.ViewModels.Pages.Patchers;
using TrxNativePatcherViewModel = TombLauncher.ViewModels.Pages.Patchers.TrxNativePatcherViewModel;

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
            .AddTransient<AiChatViewModel>()
            .AddTransient<WidescreenPatcherPageViewModel>()
            .AddTransient<TrxNativePatcherPageViewModel>()
            .AddTransient<WidescreenPatcherViewModel>()
            .AddTransient<TrxNativePatcherViewModel>()
            .AddTransient<GamepadSupportMatrixViewModel>();
    }
}
