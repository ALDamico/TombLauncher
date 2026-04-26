using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Mapping;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Database.Services;

public class GameLinkDataService
{
    private readonly TombLauncherDbContext _dbContext;
    private readonly GameLinkMapper _mapper;
    private readonly IMapper _legacyMapper;

    public GameLinkDataService(TombLauncherDbContext dbContext, GameLinkMapper mapper, IMapper legacyMapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _legacyMapper = legacyMapper;
    }

    public async Task<List<GameLinkDto>> GetLinks(int gameId, CancellationToken cancellationToken, LinkType? linkType = null)
    {
        IQueryable<GameLink> query = _dbContext.GameLink.Where(l => l.GameId == gameId);
        if (linkType != null)
            query = query.Where(g => g.LinkType == linkType);
        var result = await query.ToListAsync(cancellationToken);
        return _mapper.ToDtos(result);
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
            var shouldAdd = link == null;

            link = _mapper.ToGameLink(dto, link);
            if (shouldAdd)
            {
                _dbContext.GameLink.Add(link);
            }

            upserted.Add(link);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
        return _mapper.ToDtos(upserted);
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
            return _legacyMapper.Map<GameMetadataDto>(game);
        }

        return null;
    }

    public async Task<Dictionary<string, GameWithStatsDto>> GetGamesByLinksDictionary(
        LinkType linkType, List<string> links, List<GameWithStatsDto> gamesWithStats)
    {
        var matchingLinks = await _dbContext.GameLink
            .Where(l => links.Contains(l.Link) && l.LinkType == linkType)
            .ToListAsync();

        return matchingLinks
            .Join(gamesWithStats, l => l.GameId, game => game.GameMetadata.Id,
                (l, game) => new { l.Link, Game = game })
            .ToDictionary(k => k.Link, g => g.Game);
    }
}
