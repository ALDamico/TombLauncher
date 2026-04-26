using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Mapping;

public class GameHashMapper
{
    public GameHashDto ToDto(GameHashes hash)
    {
        return new GameHashDto()
        {
            FileName = hash.FileName,
            Md5Hash = hash.Md5Hash,
            Id = hash.Id,
            GameId = hash.GameId
        };
    }

    public List<GameHashDto> ToDtos(IEnumerable<GameHashes> hashes) => hashes.Select(ToDto).ToList();

    public GameHashes ToGameHash(GameHashDto dto)
    {
        return new GameHashes()
        {
            FileName = dto.FileName,
            Md5Hash = dto.Md5Hash,
            Id = dto.Id,
            GameId = dto.GameId
        };
    }

    public List<GameHashes> ToGameHashes(IEnumerable<GameHashDto> dtos) => dtos.Select(ToGameHash).ToList();
}