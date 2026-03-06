using AutoMapper;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Database.Services;

public class GameHashDataService
{
    private readonly TombLauncherDbContext _dbContext;
    private readonly IMapper _mapper;

    public GameHashDataService(TombLauncherDbContext dbContext, MapperConfiguration mapperConfiguration)
    {
        _dbContext = dbContext;
        _mapper = mapperConfiguration.CreateMapper();
    }

    public bool ExistsHashes(List<GameHashDto> computedHashes, out int? foundId)
    {
        foundId = null;
        var allHashes = _dbContext.GameHashes.ToList();
        var computedEntities = computedHashes.Select(h => _mapper.Map<GameHashes>(h)).ToList();

        var joined = allHashes
            .Join(computedEntities,
                gh => gh.FileName + "#" + gh.Md5Hash,
                tmp => tmp.FileName + "#" + tmp.Md5Hash,
                (gh, _) => gh);

        var matches = joined
            .GroupBy(m => m.GameId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToList();

        if (matches.Count == 0)
            return false;

        if (matches.Any(m => m.Count == computedHashes.Count))
        {
            var idToReturn = matches.FirstOrDefault()?.Id;
            var game = _dbContext.Games.Find(idToReturn.GetValueOrDefault());
            if (game == null || !game.IsInstalled)
                return false;
            foundId = idToReturn;
            return true;
        }

        return false;
    }

    public async Task SaveHashes(List<GameHashDto> hashes)
    {
        foreach (var hash in hashes)
        {
            var entity = _mapper.Map<GameHashes>(hash);
            _dbContext.GameHashes.Add(entity);
        }

        await _dbContext.SaveChangesAsync();
    }

    public List<GameHashDto> GetHashes(GameMetadataDto dto)
    {
        var queryResult = _dbContext.GameHashes
            .Where(h => h.GameId == dto.Id)
            .ToList();
        return _mapper.Map<List<GameHashDto>>(queryResult);
    }
}
