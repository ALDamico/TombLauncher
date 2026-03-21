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
        IQueryable<GameLink> query = _dbContext.GameLink.Where(l => l.GameId == gameId);
        if (linkType != null)
            query = query.Where(g => g.LinkType == linkType);
        return _mapper.Map<List<GameLinkDto>>(query.ToList());
    }

    public async Task<List<GameLinkDto>> SaveLinks(List<GameLinkDto> links, CancellationToken cancellationToken)
    {
        var upserted = new List<GameLink>();
        var linkValues = links.Select(l => l.Link).ToList();
        var existingLinks = await _dbContext.GameLink
            .Where(gl => linkValues.Contains(gl.Link))
            .ToListAsync(cancellationToken);
        var entities = links
            .GroupJoin(existingLinks, dto => new { dto.Link, dto.LinkType, dto.BaseUrl },
                gl => new { gl.Link, gl.LinkType, gl.BaseUrl }, (dto, link) => (dto, link)).ToList();
        foreach (var kvp in entities)
        {
            var link = kvp.link.FirstOrDefault();
            var dto = kvp.dto;
            if (link == null)
            {
                link = new GameLink()
                {
                    BaseUrl = dto.BaseUrl,
                    Link = dto.Link,
                    DisplayName = dto.DisplayName,
                    LinkType = dto.LinkType,
                    GameId = dto.GameId
                };
                _dbContext.GameLink.Add(link);
            }
            else
            {
                link.Link = dto.Link;
                link.BaseUrl = dto.BaseUrl;
                link.LinkType = dto.LinkType;
                link.DisplayName = dto.DisplayName;
                link.GameId = dto.GameId;
            }

            upserted.Add(link);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
        return _mapper.Map<List<GameLinkDto>>(upserted);
    }

    public async Task<GameMetadataDto?> GetGameByLinks(LinkType linkType, List<string> links)
    {
        var targetFileTypes = new List<FileType>()
        {
            FileType.GameExecutable,
            FileType.SetupExecutable,
            FileType.CommunitySetupExecutable
        };

        var gameIds = _dbContext.GameLink
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
        var queryResult = _dbContext.GameLink
            .Where(l => links.Contains(l.Link) && l.LinkType == linkType)
            .Select(l => l.GameId)
            .Join(_dbContext.Games, i => i, game => game.Id, (i, game) => game);

        return _mapper.Map<List<GameMetadataDto>>(queryResult.ToList());
    }

    public async Task<Dictionary<string, GameWithStatsDto>> GetGamesByLinksDictionary(
        LinkType linkType, List<string> links, List<GameWithStatsDto> gamesWithStats)
    {
        var matchingLinks = await _dbContext.GameLink
            .Where(l => links.Contains(l.Link) && l.LinkType == linkType)
            .ToListAsync();

        var distinctLinks = matchingLinks.DistinctBy(l => l.Link);

        return distinctLinks
            .Join(gamesWithStats, l => l.GameId, game => game.GameMetadata.Id,
                (l, game) => new { l.Link, Game = game })
            .ToDictionary(k => k.Link, g => g.Game);
    }
}
