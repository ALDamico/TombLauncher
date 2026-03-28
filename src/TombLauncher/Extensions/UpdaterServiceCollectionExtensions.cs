using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Services;

namespace TombLauncher.Extensions;

public static class UpdaterServiceCollectionExtensions
{
    public static IServiceCollection AddUpdater(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<UpdateService>();
    }
}