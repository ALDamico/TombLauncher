namespace TombLauncher.Configuration.Sections;

public interface IIntegrationsConfig
{
    string? DiscordAppId { get; set; }
    bool? SharePlaySessionsOnDiscord { get; set; }
}