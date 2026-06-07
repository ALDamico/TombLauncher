namespace TombLauncher.Configuration.Sections;

public class IntegrationsConfig : IIntegrationsConfig
{
    public string? DiscordAppId { get; set; }
    public bool? SharePlaySessionsOnDiscord { get; set; }
}