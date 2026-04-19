using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Installers;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Services;

namespace TombLauncher.Extensions;

public static class GameSearchServiceCollectionExtensions
{
    public static IServiceCollection AddGameInstallationFeatures(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddScoped<TombRaiderLevelInstaller>()
            .AddScoped<TombRaiderEngineDetector>()
            .AddTransient<IGameMerger>(_ =>
                new TombLauncherGameMerger(new GameSearchResultMetadataDistanceCalculator()
                    { UseAuthor = true, IgnoreSubTitle = true }))
            .AddTransient(sp =>
            {
                var downloadManager = new GameDownloadManager(sp.GetRequiredService<IGameMerger>())
                {
                    Downloaders = sp.GetRequiredService<ISettingsProvider>().GetActiveDownloaders()
                };

                return downloadManager;
            })
            .AddScoped(_ => new GameFileHashCalculator(new HashSet<string>()
            {
                ".tr4",
                ".pak",
                ".tr2",
                ".sfx",
                ".dat",
                ".phd"
            }))
            .AddSingleton<IAppFileOperationsService, AppFileOperationsService>();
    }
}