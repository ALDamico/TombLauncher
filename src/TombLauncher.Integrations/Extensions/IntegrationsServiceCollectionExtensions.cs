using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Integrations.Discord;

namespace TombLauncher.Integrations.Extensions;

public static class IntegrationsServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordIntegration(this IServiceCollection services, string applicationId)
    {
        return services.AddSingleton<DiscordRichPresenceService>(_ => new DiscordRichPresenceService(applicationId));
    }  
}