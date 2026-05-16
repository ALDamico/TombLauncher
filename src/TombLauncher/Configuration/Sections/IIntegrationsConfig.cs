using TombLauncher.Contracts.Integrations;

namespace TombLauncher.Configuration.Sections;

public interface IIntegrationsConfig : IDiscordConfiguration
{
    bool? SharePlaySessionsOnDiscord { get; set; }
}