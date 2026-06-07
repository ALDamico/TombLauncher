using DiscordRPC;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Integrations;

namespace TombLauncher.Integrations.Discord;

public class DiscordRichPresenceService : IDisposable
{
    public DiscordRichPresenceService(IDiscordConfiguration config)
    {
        _applicationId = config.DiscordAppId ?? string.Empty;
    }
    private DiscordRpcClient? _discordClient;
    private bool _isInitialized;
    private readonly string _applicationId;

    public void UpdateStatus(RichPresenceDto richPresenceDto)
    {
        if (!_isInitialized)
        {
            _discordClient = new DiscordRpcClient(_applicationId);
            _discordClient.Initialize();
            _isInitialized = true;
        }
        
        var richPresence = new RichPresence()
        {
            Type = ActivityType.Playing,
            Details = $"Playing {richPresenceDto.LevelName}",
            State = $"by {richPresenceDto.AuthorName}",
            Buttons = [],
            Assets = new Assets()
            {
                LargeImageUrl = richPresenceDto.LevelUrl,
                LargeImageText = $"Try {richPresenceDto.LevelName}",
                LargeImageKey = GetLogoToUse(richPresenceDto.Engine)
            },
        };

        var buttons = new List<Button>();

        if (richPresenceDto is { LevelUrl: not null })
        {
            buttons.Add(new Button(){Label = richPresenceDto.LevelName, Url = richPresenceDto.LevelUrl});
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

    private string GetLogoToUse(GameEngine engine)
    {
        if (engine.HasFlag(GameEngine.Tr1x) || engine.HasFlag(GameEngine.Tr2x) || engine.HasFlag(GameEngine.Trx))
            return Constants.TrxLogoAsset;

        if (engine.HasFlag(GameEngine.Ten))
            return Constants.TenLogoAsset;

        if (engine.HasFlag(GameEngine.TombRaider1))
            return Constants.Tr1LogoAsset;

        if (engine.HasFlag(GameEngine.TombRaider2))
            return Constants.Tr2LogoAsset;
        
        if (engine.HasFlag(GameEngine.TombRaider3))
            return Constants.Tr3LogoAsset;
        
        if (engine.HasFlag(GameEngine.TombRaider4))
            return Constants.Tr4LogoAsset;
        
        if (engine.HasFlag(GameEngine.TombRaider5))
            return Constants.Tr5LogoAsset;

        return Constants.TombLauncherLogoAsset;
    }
}