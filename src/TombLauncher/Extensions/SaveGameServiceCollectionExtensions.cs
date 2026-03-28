using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TombLauncher.Core.Savegames;
using TombLauncher.Services;

namespace TombLauncher.Extensions;

public static class SaveGameServiceCollectionExtensions
{
    public static IServiceCollection AddSaveGameManagement(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ISavegameHeaderProvider, SavegameHeaderProvider>();
        serviceCollection.AddTransient<SavegameQueryService>();
        serviceCollection.AddTransient<SavegameCommandService>();
        serviceCollection.AddScoped(sp =>
        {
            var settingsProvider = sp.GetRequiredService<ISettingsProvider>();
            var delay = settingsProvider.GetSavegameSettings().ProcessingDelay;
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return new SavegameHeaderProcessor(loggerFactory.CreateLogger<SavegameHeaderProcessor>())
            {
                Delay = delay
            };
        });
        return serviceCollection;
    }
}