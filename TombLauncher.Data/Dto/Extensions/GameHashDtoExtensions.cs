using TombLauncher.Data.Models;

namespace TombLauncher.Data.Dto.Extensions;

public static class GameHashDtoExtensions
{
    public static GameHashDto ToDto(this GameHashes hash)
    {
        return new GameHashDto()
        {
            Id = hash.Id,
            GameId = hash.GameId,
            FileName = hash.FileName,
            Md5Hash = hash.Md5Hash
        };
    }

    public static IEnumerable<GameHashDto> ToDtos(this IEnumerable<GameHashes> hashes) => hashes.Select(ToDto);
}