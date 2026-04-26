using Microsoft.EntityFrameworkCore;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Mapping;

namespace TombLauncher.Data.Database.Services;

public class GameHashDataService
{
    private readonly TombLauncherDbContext _dbContext;
    private readonly GameHashMapper _mapper;

    public GameHashDataService(TombLauncherDbContext dbContext, GameHashMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<(bool, int?)> ExistsHashes(List<GameHashDto> computedHashes, CancellationToken cancellationToken)
    {
        var allHashes = await _dbContext.GameHashes.ToListAsync(cancellationToken);

        var joined = allHashes
            .Join(computedHashes,
                gh => gh.FileName + "#" + gh.Md5Hash,
                tmp => tmp.FileName + "#" + tmp.Md5Hash,
                (gh, _) => gh);

        var matches = joined
            .GroupBy(m => m.GameId)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToList();

        if (matches.Count == 0)
            return (false, null);

        if (matches.Any(m => m.Count == computedHashes.Count))
        {
            var idToReturn = matches.FirstOrDefault()?.Id;
            var game = await _dbContext.Games.FindAsync([idToReturn.GetValueOrDefault()], cancellationToken);
            return game is not { IsInstalled: true } ? (false, null) : (true, idToReturn);
        }

        return (false, null);
    }

    public async Task SaveHashes(List<GameHashDto> hashes)
    {
        var allHashes = _mapper.ToGameHashes(hashes);
        await _dbContext.GameHashes.AddRangeAsync(allHashes);

        await _dbContext.SaveChangesAsync();
    }
}
