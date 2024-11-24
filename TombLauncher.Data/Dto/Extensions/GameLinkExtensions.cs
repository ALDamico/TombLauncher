using TombLauncher.Data.Models;

namespace TombLauncher.Data.Dto.Extensions;

public static class GameLinkExtensions
{
    public static GameLinkDto ToDto(this GameLink link)
    {
        return new GameLinkDto()
        {
            Id = link.Id,
            GameId = link.GameId,
            LinkType = link.LinkType,
            Link = link.Link
        };
    }

    public static IEnumerable<GameLinkDto> ToDtos(this IEnumerable<GameLink> links) => links.Select(ToDto);

    public static GameLink ToGameLink(this GameLinkDto dto)
    {
        return new GameLink()
        {
            Id = dto.Id,
            GameId = dto.GameId,
            LinkType = dto.LinkType,
            Link = dto.Link
        };
    }
}