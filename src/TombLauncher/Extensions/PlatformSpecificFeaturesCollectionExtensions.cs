using System;
using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Configuration;
using TombLauncher.Configuration.Sections;
using TombLauncher.Core.Launchers;
using TombLauncher.Core.PlatformSpecific;

namespace TombLauncher.Extensions;

public static class PlatformSpecificFeaturesCollectionExtensions
{
    public static IServiceCollection AddPlatformSpecificFeatures(
        this IServiceCollection services,
        IPlatformSpecificFeatures platformSpecificFeatures)
    {
        services.AddSingleton(platformSpecificFeatures);

        services.AddTransient<IGameLauncher>(sp =>
        {
            var cfg = sp.GetRequiredService<IAppConfiguration>().Compatibility;
            return cfg.CompatibilityTool switch
            {
                CompatibilityTool.Proton => new ProtonGameLauncher(cfg.ProtonPath ?? ""),
                CompatibilityTool.None => new WindowsGameLauncher(),
                _ => new WineGameLauncher(cfg.WinePath ?? "wine"),
            };
        });
        // Factory so consumers (e.g. GameWithStatsService) can re-resolve the
        // launcher on each use, reflecting any compatibility tool change since startup.
        services.AddTransient<Func<IGameLauncher>>(sp => () => sp.GetRequiredService<IGameLauncher>());

        return services;
    }
}
