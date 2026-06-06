using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Patchers.Tomb4Plus.Extractors;
using TombLauncher.Patchers.Tomb4Plus.Parsers;
using TombLauncher.Patchers.Tomb4Plus.Services;
using TombLauncher.Patchers.Trx.Patchers;
using TombLauncher.Patchers.Widescreen;
using TombLauncher.Services.Patchers.Tomb4Plus;
using TombLauncher.Services.Patchers.TrxNative;
using TombLauncher.Services.Patchers.Widescreen;

namespace TombLauncher.Extensions;

public static class PatchersServiceCollectionExtensions
{
    public static IServiceCollection AddPatchers(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpClient<Tomb4PlusPatcherService>();
        return serviceCollection.AddSingleton<WidescreenPatcherService>()
            .AddSingleton<WidescreenPatcher>()
            .AddSingleton<TrxNativePatcherService>()
            .AddSingleton<TrxNativeExecutablePatcher>()
            .AddSingleton<TrlePatchExtractor>()
            .AddSingleton<LeikkuriParser>()
            .AddSingleton<EsseParser>()
            .AddSingleton<FurrSyntaxParser>()
            .AddSingleton<Tomb4PlusFeatureExtractorService>()
            .AddSingleton<Tomb4PlusPatcherService>();
    }
}