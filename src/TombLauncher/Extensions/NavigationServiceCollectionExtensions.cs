using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Services;

namespace TombLauncher.Extensions;

public static class NavigationServiceCollectionExtensions
{
    public static IServiceCollection AddNavigation(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton(sp => new NavigationManager(sp));
    }
}