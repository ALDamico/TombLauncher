﻿using AutoMapper;
using TombLauncher.Data.Database.Repositories;
using TombLauncher.Data.Dto;
using TombLauncher.Data.Dto.Extensions;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Database.UnitOfWork;

public class GamesUnitOfWork : UnitOfWorkBase
{
    public GamesUnitOfWork(MapperConfiguration mapperConfiguration)
    {
        _mapper = mapperConfiguration.CreateMapper();
        _games = GetRepository<Game>();
        _playSessions = GetRepository<PlaySession>();
        _hashes = GetRepository<GameHashes>();
        _links = GetRepository<GameLink>();
    }

    private IMapper _mapper;

    private readonly Lazy<EfRepository<Game>> _games;
    private readonly Lazy<EfRepository<PlaySession>> _playSessions;
    private readonly Lazy<EfRepository<GameHashes>> _hashes;
    private readonly Lazy<EfRepository<GameLink>> _links;

    internal EfRepository<Game> Games => _games.Value;

    internal EfRepository<PlaySession> PlaySessions => _playSessions.Value;
    internal EfRepository<GameHashes> Hashes => _hashes.Value;
    internal EfRepository<GameLink> Links => _links.Value;


    public void UpsertGame(GameMetadataDto game)
    {
        var entity = ToGame(game);
        if (entity.Id == default)
        {
            Games.Insert(entity);
        }
        else
        {
            Games.Update(entity);
        }

        Save();
        game.Id = entity.Id;
    }

    public void DeleteGameById(int id)
    {
        Games.Delete(id);
    }

    private static Game ToGame(GameMetadataDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentException("Dto can't be null", nameof(dto));
        }

        return new Game()
        {
            Id = dto.Id,
            Author = dto.Author,
            Length = dto.Length,
            Difficulty = dto.Difficulty,
            Setting = dto.Setting,
            Title = dto.Title,
            ExecutablePath = dto.ExecutablePath,
            GameEngine = dto.GameEngine,
            InstallDate = dto.InstallDate,
            InstallDirectory = dto.InstallDirectory,
            ReleaseDate = dto.ReleaseDate,
            Description = dto.Description,
            Guid = dto.Guid,
            TitlePic = dto.TitlePic,
            AuthorFullName = dto.AuthorFullName
        };
    }

    private GameHashes ToGameHashes(GameHashDto dto)
    {
        return _mapper.Map<GameHashes>(dto);
    }

    public List<GameMetadataDto> GetGames()
    {
        return _mapper.Map<List<GameMetadataDto>>(Games.GetAll());
    }

    public List<GameWithStatsDto> GetGamesWithStats()
    {
        var outputList = new List<GameWithStatsDto>();
        var playSessions = PlaySessions.GetAll().ToLookup(ps => ps.GameId);

        var games = Games.GetAll().ToList();
        foreach (var game in games)
        {
            var thisGamePlaySessions = playSessions[game.Id].ToList();
            var gameWithStatsDto = new GameWithStatsDto()
            {
                GameMetadata = _mapper.Map<GameMetadataDto>(game),
                TotalPlayTime = TimeSpan.Zero
            };

            if (thisGamePlaySessions.Any())
            {
                gameWithStatsDto.LastPlayed = thisGamePlaySessions.Max(ps => ps.StartDate);
                gameWithStatsDto.TotalPlayTime =
                    TimeSpan.FromTicks(thisGamePlaySessions.Select(ps => (ps.EndDate - ps.StartDate).Ticks).Sum());
            }

            outputList.Add(gameWithStatsDto);
        }

        return outputList;
    }

    public List<PlaySessionDto> GetPlaySessionsByGameId(int gameId)
    {
        return PlaySessions.Get(ps => ps.Game.Id == gameId).AsEnumerable().ToDtos().ToList();
    }

    public void AddPlaySessionToGame(GameMetadataDto dto, DateTime startDate, DateTime endDate)
    {
        var gameId = dto.Id;
        var playSession = new PlaySession()
        {
            GameId = gameId,
            StartDate = startDate,
            EndDate = endDate
        };
        PlaySessions.Insert(playSession);
    }

    public PlaySessionDto GetLastPlaySession(GameMetadataDto dto)
    {
        var gameId = dto.Id;
        var lastPlaysession = PlaySessions
            .Get(ps => ps.GameId == gameId, playSessions => playSessions.OrderByDescending(ps => ps.EndDate))
            .FirstOrDefault();
        return lastPlaysession.ToDto();
    }

    public List<GameHashDto> GetHashes(GameMetadataDto dto)
    {
        var queryResult = Hashes.Get(h => h.GameId == dto.Id); //.AsEnumerable().ToDtos().ToList();
        return _mapper.Map<List<GameHashDto>>(queryResult);
    }

    public bool ExistsHashes(List<GameHashDto> computedHashes)
    {
        var hashesRepo = Hashes.GetAll();
        var tempQueryable = computedHashes.Select(ToGameHashes).AsQueryable();
        var joined = hashesRepo.AsEnumerable().Join(tempQueryable, gh => gh.FileName + "#" + gh.Md5Hash,
            tmp => tmp.FileName + "#" + tmp.Md5Hash,
            (gh, tmp) => gh);
        var matches = joined.GroupBy(m => m.GameId).Select(g => new { Id = g.Key, Count = g.Count() }).ToList();
        if (matches.Count == 0)
        {
            return false;
        }

        if (matches.Any(m => m.Count == computedHashes.Count))
        {
            return true;
        }

        return false;
    }

    public void SaveHashes(List<GameHashDto> hashes)
    {
        foreach (var hash in hashes)
        {
            Hashes.Insert(ToGameHashes(hash));
        }

        Save();
    }

    public List<GameLinkDto> GetLinks(int gameId)
    {
        var queryResult = Links.Get(l => l.GameId == gameId);
        return _mapper.Map<List<GameLinkDto>>(queryResult);
    }

    public void SaveLink(GameLinkDto dto)
    {
        if (dto.Id != default) return;

        var entity = _mapper.Map<GameLink>(dto);
        Links.Upsert(entity);
    }

    public GameMetadataDto GetGameByLinks(LinkType linkType, List<string> links)
    {
        var gameIds = Links.Get(l => links.Contains(l.Link) && l.LinkType == linkType).Select(l => l.GameId).Distinct()
            .ToList();
        if (gameIds.Count == 1)
        {
            return _mapper.Map<GameMetadataDto>(Games.GetEntityById(gameIds.First()));
        }

        return null;
    }

    public List<GameMetadataDto> GetGamesByLinks(LinkType linkType, List<string> links)
    {
        var games = Games.GetAll();
        var queryResult = Links.Get(l => links.Contains(l.Link) && l.LinkType == linkType)
            .Select(l => l.GameId)
            .Join(games, i => i, game => game.Id, (i, game) => game);

        return _mapper.Map<List<GameMetadataDto>>(queryResult);
    }

    public Dictionary<string, GameMetadataDto> GetGamesByLinksDictionary(LinkType linkType, List<string> links)
    {
        var games = Games.GetAll();
        return Links.Get(l => links.Contains(l.Link) && l.LinkType == linkType)
            .Join(games, l => l.GameId, game => game.Id, (i, game) => new { Link = i.Link, Game = game })
            .ToDictionary(k => k.Link, g => _mapper.Map<GameMetadataDto>(g.Game));
    }
}