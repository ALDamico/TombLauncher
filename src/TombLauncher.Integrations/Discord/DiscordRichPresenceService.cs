using DiscordRPC;
using TombLauncher.Contracts.Integrations;

namespace TombLauncher.Integrations.Discord;

public class DiscordRichPresenceService : IDisposable
{
    private DiscordRpcClient? _discordClient;
    private bool _isInitialized;

    public void UpdateStatus(RichPresenceDto richPresenceDto)
    {
        if (!_isInitialized)
        {
            _discordClient = new DiscordRpcClient(richPresenceDto.DiscordAppId);
            _discordClient.Initialize();
            _isInitialized = true;
        }
        var richPresence = new RichPresence()
        {
            Type = ActivityType.Playing,
            Details = richPresenceDto.Title,
            State = richPresenceDto.State,
            Buttons = [],
            Assets = new Assets()
            {
                LargeImageUrl = richPresenceDto.ScreenshotUrl,
                LargeImageText = richPresenceDto.Title,
                LargeImageKey = "tomb-launcher-logo"
            },
            
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
        _discordClient!.SetPresence(richPresence);
    }

    public void EndPlaySession()
    {
        if (_isInitialized)
            _discordClient!.ClearPresence();
    }

    public void Dispose()
    {
        _discordClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}