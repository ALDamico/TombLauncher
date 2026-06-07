using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Contracts.Integrations;
using TombLauncher.Integrations.Discord;

namespace TombLauncher.Integrations.Extensions;

public static class IntegrationsServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordIntegration(this IServiceCollection services)
    {
        return services.AddSingleton<DiscordRichPresenceService>(sp =>
            new DiscordRichPresenceService(sp.GetRequiredService<IDiscordConfiguration>()));
    }
}