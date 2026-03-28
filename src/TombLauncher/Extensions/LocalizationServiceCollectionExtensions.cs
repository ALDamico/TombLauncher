using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Contracts.Localization;
using TombLauncher.Localization;

namespace TombLauncher.Extensions;

public static class LocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddLocalization(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<ILocalizationManager>(_ => new LocalizationManager(Application.Current!));
    }
}