using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Mapping;

public class GameLinkMapper
{
    public GameLinkDto ToDto(GameLink gameLink)
    {
        return new GameLinkDto()
        {
            DisplayName = gameLink.DisplayName,
            BaseUrl = gameLink.BaseUrl,
            Link = gameLink.Link,
            Id = gameLink.Id,
            GameId = gameLink.GameId,
            LinkType = gameLink.LinkType
        };
    }

    public List<GameLinkDto> ToDtos(IEnumerable<GameLink> links) => links.Select(ToDto).ToList();

    public GameLink ToGameLink(GameLinkDto dto, GameLink? extant = null)
    {
        if (extant == null)
        {
            extant = new GameLink()
            {
                BaseUrl = "",
                Link = "",
                DisplayName = "",
                Id = dto.Id
            };
        }

        extant.DisplayName = dto.DisplayName;
        extant.BaseUrl = dto.BaseUrl;
        extant.Link = dto.Link;
        extant.LinkType = dto.LinkType;
        extant.GameId = dto.GameId;
        return extant;
    }
}