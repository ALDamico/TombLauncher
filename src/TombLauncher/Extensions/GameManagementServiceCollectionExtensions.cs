using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Contracts.EngineDetectors;
using TombLauncher.Installers;
using TombLauncher.Installers.Downloaders;

namespace TombLauncher.Extensions;

public static class GameManagementServiceCollectionExtensions
{
    public static IServiceCollection AddGameManagement(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<TombRaiderLevelInstaller>();
        serviceCollection.AddSingleton<IEngineDetector, TombRaiderEngineDetector>();
        serviceCollection.AddTransient<IGameMerger>(_ =>
            new TombLauncherGameMerger(new GameSearchResultMetadataDistanceCalculator()
                { UseAuthor = true, IgnoreSubTitle = true }));
        return serviceCollection;
    }
}