using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Services;

namespace TombLauncher.Extensions;

public static class ThemingServiceCollectionExtensions
{
    public static IServiceCollection AddTheming(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<ThemeManager>();
    }
}