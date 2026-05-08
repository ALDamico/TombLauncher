using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TombLauncher.Core.Savegames;
using TombLauncher.Services;

namespace TombLauncher.Extensions;

public static class SavegamesServiceCollectionExtensions
{
    public static IServiceCollection AddSavegameManagement(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<ISavegameHeaderProvider, SavegameHeaderProvider>()
            .AddTransient<SavegameQueryService>()
            .AddTransient<SavegameCommandService>()
            .AddScoped(sp =>
        {
            var settingsProvider = sp.GetRequiredService<ISettingsProvider>();
            var delay = settingsProvider.GetSavegameSettings().ProcessingDelay;
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return new SavegameHeaderProcessor(loggerFactory.CreateLogger<SavegameHeaderProcessor>())
            {
                Delay = delay
            };
        });
    }
}