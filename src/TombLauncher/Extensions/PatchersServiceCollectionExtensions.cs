using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Patchers.Widescreen;
using TombLauncher.Services.Patchers.Widescreen;

namespace TombLauncher.Extensions;

public static class PatchersServiceCollectionExtensions
{
    public static IServiceCollection AddPatchers(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<WidescreenPatcherService>()
            .AddSingleton<WidescreenPatcher>();
    }
}