using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Database.Services;

public class GameLinkDataService
{
    private readonly TombLauncherDbContext _dbContext;
    private readonly IMapper _mapper;

    public GameLinkDataService(TombLauncherDbContext dbContext, MapperConfiguration mapperConfiguration)
    {
        _dbContext = dbContext;
        _mapper = mapperConfiguration.CreateMapper();
    }

    public List<GameLinkDto> GetLinks(int gameId, LinkType? linkType = null)
    {
        IQueryable<GameLink> query = _dbContext.GameLinks.Where(l => l.GameId == gameId);
        if (linkType != null)
            query = query.Where(g => g.LinkType == linkType);
        return _mapper.Map<List<GameLinkDto>>(query.ToList());
    }

    public async Task SaveLink(GameLinkDto dto)
    {
        if (dto.Id != 0) return;

        var entity = await _dbContext.GameLinks
            .Where(l => l.Link == dto.Link && l.LinkType == dto.LinkType && l.BaseUrl == dto.BaseUrl)
            .FirstOrDefaultAsync();

        if (entity == null)
        {
            entity = _mapper.Map<GameLink>(dto);
            _dbContext.GameLinks.Add(entity);
        }
        else
        {
            _mapper.Map(dto, entity);
            _dbContext.GameLinks.Update(entity);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<GameMetadataDto?> GetGameByLinks(LinkType linkType, List<string> links)
    {
        var targetFileTypes = new List<FileType>()
        {
            FileType.GameExecutable,
            FileType.SetupExecutable,
            FileType.CommunitySetupExecutable
        };

        var gameIds = _dbContext.GameLinks
            .Where(l => links.Contains(l.Link) && l.LinkType == linkType)
            .Select(l => l.GameId)
            .Distinct()
            .ToList();

        if (gameIds.Count == 1)
        {
            var gameId = gameIds.First();
            var game = await _dbContext.Games
                .Include(g => g.FileBackups.Where(f => targetFileTypes.Contains(f.FileType)))
                .SingleAsync(g => g.Id == gameId);
            return _mapper.Map<GameMetadataDto>(game);
        }

        return null;
    }

    public List<GameMetadataDto> GetGamesByLinks(LinkType linkType, List<string> links)
    {
        var queryResult = _dbContext.GameLinks
            .Where(l => links.Contains(l.Link) && l.LinkType == linkType)
            .Select(l => l.GameId)
            .Join(_dbContext.Games, i => i, game => game.Id, (i, game) => game);

        return _mapper.Map<List<GameMetadataDto>>(queryResult.ToList());
    }

    public async Task<Dictionary<string, GameWithStatsDto>> GetGamesByLinksDictionary(
        LinkType linkType, List<string> links, List<GameWithStatsDto> gamesWithStats)
    {
        var matchingLinks = await _dbContext.GameLinks
            .Where(l => links.Contains(l.Link) && l.LinkType == linkType)
            .ToListAsync();

        return matchingLinks
            .Join(gamesWithStats, l => l.GameId, game => game.GameMetadata.Id,
                (l, game) => new { l.Link, Game = game })
            .ToDictionary(k => k.Link, g => g.Game);
    }
}
