using System.Collections.Generic;
using System.Linq;
using TombLauncher.Models;

namespace TombLauncher.Dto.Extensions;

public static class PlaySessionDtoExtensions
{
    public static PlaySessionDto ToDto(this PlaySession playSession)
    {
        return new PlaySessionDto()
        {
            Id = playSession.Id,
            GameId = playSession.Game.Id,
            StartDate = playSession.StartDate,
            EndDate = playSession.EndDate
        };
    }

    public static IEnumerable<PlaySessionDto> ToDtos(this IEnumerable<PlaySession> playSessions) =>
        playSessions.Select(ToDto);
}