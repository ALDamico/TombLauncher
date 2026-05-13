using DiscordRPC;
using TombLauncher.Contracts.Integrations;

namespace TombLauncher.Integrations.Discord;

public class DiscordRichPresenceService : IDisposable
{
    public DiscordRichPresenceService(string applicationId)
    {
        _discordClient = new DiscordRpcClient(applicationId);
        _discordClient.Initialize();
    }
    private readonly DiscordRpcClient _discordClient;

    public void UpdateStatus(RichPresenceDto richPresenceDto)
    {
        var richPresence = new RichPresence()
        {
            Type = ActivityType.Playing,
            Details = richPresenceDto.Title,
            Buttons = [],
            Assets =
            {
                LargeImageUrl = richPresenceDto.ScreenshotUrl,
                LargeImageText = richPresenceDto.LevelName
            },
            Timestamps = new Timestamps(richPresenceDto.StartTimestamp)
        };

        var buttons = new List<Button>();

        if (richPresenceDto is { LevelCaption: not null, LevelUrl: not null })
        {
            buttons.Add(new Button(){Label = richPresenceDto.LevelCaption, Url = richPresenceDto.LevelUrl});
        }

        if (richPresenceDto is { WebsiteCaption: not null, WebsiteUrl: not null })
        {
            buttons.Add(new Button(){Label = richPresenceDto.WebsiteCaption, Url = richPresenceDto.WebsiteUrl});
        }

        richPresence.Buttons = buttons.ToArray();
        _discordClient.SetPresence(richPresence);
    }

    public void EndPlaySession() => _discordClient.ClearPresence();

    public void Dispose()
    {
        _discordClient.Dispose();
        GC.SuppressFinalize(this);
    }
}