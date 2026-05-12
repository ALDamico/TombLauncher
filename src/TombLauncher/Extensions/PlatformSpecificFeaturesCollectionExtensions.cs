using System;
using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Launchers;
using TombLauncher.Core.PlatformSpecific;

namespace TombLauncher.Extensions;

public static class PlatformSpecificFeaturesCollectionExtensions
{
    public static IServiceCollection AddPlatformSpecificFeatures(
        this IServiceCollection services,
        IPlatformSpecificFeatures platformSpecificFeatures)
    {
        services.AddSingleton(platformSpecificFeatures)
            .AddTransient<IGameLauncher>(sp =>
            {
                var cfg = sp.GetRequiredService<ILayeredAppConfiguration>().Compatibility;

                var platform = sp.GetRequiredService<IPlatformSpecificFeatures>();

                if (platform.Platform == Platform.Windows)
                    return new WindowsGameLauncher();

                return cfg.CompatibilityTool switch
                {
                    CompatibilityTool.Proton => new ProtonGameLauncher(cfg.ProtonPath ?? ""),
                    CompatibilityTool.WindowsNative => new WindowsGameLauncher(),
                    CompatibilityTool.Wine => new WineGameLauncher(cfg.WinePath ?? "wine"),
                    _ => platform.Platform == Platform.Windows ? new WindowsGameLauncher() : new WineGameLauncher(cfg.WinePath ?? "wine")
                };
            });
        // Factory so consumers (e.g. GameWithStatsService) can re-resolve the
        // launcher on each use, reflecting any compatibility tool change since startup.
        services.AddTransient<Func<IGameLauncher>>(sp => sp.GetRequiredService<IGameLauncher>);

        return services;
    }
}
