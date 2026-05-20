using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Installers.Downloaders.TRCustoms.org.Requests;
using TombLauncher.Installers.Downloaders.TRCustoms.org.Responses;
using TombLauncher.Installers.Downloaders.TRCustoms.org.Utils;

namespace TombLauncher.Installers.Downloaders.TRCustoms.org;

public class TrCustomsGameDownloader : GameDownloaderBase
{
    public override string DisplayName => "TRCustoms.org";
    public override string BaseUrl => "https://trcustoms.org/";

    public TrCustomsGameDownloader(IHttpClientFactory httpClientFactory, ILogger<TrCustomsGameDownloader> logger) : base(httpClientFactory, logger)
    {
        _jsonSerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        _ratingMap = new Dictionary<string, int>()
        {
            { "Masterpiece", 10 },
            { "Overwhelmingly positive", 10 },
            { "Very positive", 9 },
            { "Positive", 8 },
            { "Slightly positive", 7 },
            { "Mixed", 6 },
            { "Slightly negative", 5 },
            { "Negative", 4 },
            { "Very negative", 3 },
            { "Overwhelmingly negative", 2 },
            { "Failure", 1 }
        };
    }

    private readonly JsonSerializerSettings _jsonSerializerSettings;

    private Dictionary<int, GameEngine> _enginesMap = new();
    private Dictionary<GameEngine, int> _reverseEnginesMap = new();
    private Dictionary<string, LevelTagResponse> _tagsMap = new();
    private Dictionary<string, LevelGenreResponse> _genresMap = new();
    private readonly Dictionary<string, int> _ratingMap;

    public override async Task<ISearchResultPage> GetGames(DownloaderSearchPayload payload, int page, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_enginesMap.Count == 0)
        {
            var engineMapTask = FetchSupportedEngines(cancellationToken);
            var genreMapTask = FetchGenres(cancellationToken);
            var tagMapTask = FetchTags(cancellationToken);
            await Task.WhenAll(engineMapTask, genreMapTask, tagMapTask);
            _enginesMap = engineMapTask.Result;
            _reverseEnginesMap = _enginesMap.ToDictionary(k => k.Value, k => k.Key);
            _tagsMap = tagMapTask.Result;
            _genresMap = genreMapTask.Result;
        }

        return await FetchPage(payload, page, cancellationToken);
    }

    private async Task<TrCustomsPagedResponse<T>> GetPagedResponse<T>(string endpoint, IEnumerable<KeyValuePair<string, string?>>? trCustomsRequest = null,
        CancellationToken cancellationToken = default)
    {
        var relativeUri = new UriBuilder(HttpClient.BaseAddress!);
        relativeUri.Path = $"api/{endpoint}/";
        var completeUri = relativeUri.Uri.ToString();
        if (trCustomsRequest != null)
        {
            completeUri = QueryHelpers.AddQueryString(relativeUri.Uri.ToString(), trCustomsRequest);
        }
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(completeUri)
        };
        using var response = await HttpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var pagedResponse = JsonConvert.DeserializeObject<TrCustomsPagedResponse<T>>(
            await response.Content.ReadAsStringAsync(cancellationToken), _jsonSerializerSettings);
        return pagedResponse ?? new TrCustomsPagedResponse<T>();
    }

    private async Task<Dictionary<string, LevelTagResponse>> FetchTags(CancellationToken cancellationToken)
    {
        // This will probably need to be changed if the number of tags and/or genres tends to grow quite quickly.
        var request = new TrCustomsBaseRequest()
        {
            PageSize = 500
        };
        var dictified = RequestUtils.DictifyRequest(request);

        var tags = await GetPagedResponse<LevelTagResponse>("level_tags", dictified, cancellationToken);
        return tags.Results.ToDictionary(t => t.Name?.ToUpperInvariant() ?? string.Empty, t => t);
    }

    private async Task<Dictionary<string, LevelGenreResponse>> FetchGenres(CancellationToken cancellationToken)
    {
        var request = new TrCustomsBaseRequest()
        {
            PageSize = 50
        };
        var dictified = RequestUtils.DictifyRequest(request);
        var genres = await GetPagedResponse<LevelGenreResponse>("level_genres", dictified, cancellationToken);
        return genres.Results?.ToDictionary(g => g.Name?.ToUpperInvariant() ?? string.Empty, g => g) ?? new Dictionary<string, LevelGenreResponse>();
    }

    private async Task<Dictionary<int, GameEngine>> FetchSupportedEngines(CancellationToken cancellationToken)
    {
        var engines = await GetPagedResponse<LevelEngineResponse>("level_engines", cancellationToken: cancellationToken);

        var dict = new Dictionary<int, GameEngine>();
        if (engines.TotalCount < 1)
            return dict;

        foreach (var response in engines.Results)
        {
            var engine = response.Name switch
            {
                "TR1" => GameEngine.TombRaider1,
                "TR2" => GameEngine.TombRaider2,
                "TR3" => GameEngine.TombRaider3,
                "TR4" => GameEngine.TombRaider4,
                "TR5" => GameEngine.TombRaider5,
                "TEN" => GameEngine.Ten,
                _ => GameEngine.Unknown
            };

            dict[response.Id] = engine;
        }

        return dict;
    }

    private void ParseResultPage(List<LevelSummaryResponse> levelSummaries, List<IGameSearchResultMetadata> result,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (var summary in levelSummaries)
        {
            string? authors = null;
            if (summary.Authors.IsNotNullOrEmpty())
            {
                authors = string.Join(", ", summary.Authors.Select(a => a.Username));
            }

            var tags = summary.Tags?.Select(t => t.Name)?.ToArray() ?? [];
            var settingString = tags.Length == 0 ? string.Join(", ", tags) : null;

            var rating = ParseRating(summary.RatingClass);

            var difficulty = ParseDifficulty(summary.Difficulty);

            var gameEngine = ParseGameEngine(summary.Engine);

            var metadata = new GameSearchResultMetadataDto()
            {
                BaseUrl = BaseUrl,
                SourceSiteDisplayName = DisplayName,
                Author = authors ?? string.Empty,
                Rating = rating,
                Description = summary.Description ?? string.Empty,
                Difficulty = difficulty,
                Engine = gameEngine,
                Length = ParseDuration(summary.Duration),
                Setting = settingString ?? string.Empty,
                Title = summary.Name ?? string.Empty,
                ReleaseDate = summary.Created,
                TitlePic = summary.Cover?.Url ?? string.Empty,
                SizeInMb = summary.LastFile?.Size != null ? (int)Math.Ceiling(summary.LastFile.Size / (1024.0 * 1024.0)) : null,
                ReviewCount = summary.ReviewCount,
                ReviewsLink = $"levels/{summary.Id}/reviews".EnsureStartsWith(BaseUrl, '/'),
                DetailsLink = $"levels/{summary.Id}".EnsureStartsWith(BaseUrl, '/'),
                DownloadLink = summary.LastFile?.Url.EnsureStartsWith(BaseUrl, '/') ?? string.Empty
            };
            result.Add(metadata);
        }
    }

    private double? ParseRating(RatingClassResponse? ratingClass)
    {
        double? rating = null;
        if (ratingClass is { Name: not null } && _ratingMap.TryGetValue(ratingClass.Name, out var value))
        {
            rating = value;
        }

        return rating;
    }

    private GameDifficulty ParseDifficulty(LevelDifficultyResponse? difficultyResponse)
    {
        var difficulty = GameDifficulty.Unknown;
        if (difficultyResponse != null)
        {
            difficulty = (GameDifficulty)difficultyResponse.Id;
        }

        return difficulty;
    }

    private GameEngine ParseGameEngine(LevelEngineResponse? engine)
    {
        var gameEngine = GameEngine.Unknown;
        if (engine != null)
        {
            gameEngine = _enginesMap[engine.Id];
        }

        return gameEngine;
    }

    private GameLength ParseDuration(LevelDurationResponse? levelDurationResponse)
    {
        var durationName = levelDurationResponse?.Name;
        if (durationName == null) 
            return GameLength.Unknown;

        if (durationName.StartsWith("Short", StringComparison.InvariantCultureIgnoreCase))
        {
            return GameLength.Short;
        }

        if (durationName.StartsWith("medium", StringComparison.InvariantCultureIgnoreCase))
        {
            return GameLength.Medium;
        }

        if (durationName.StartsWith("long", StringComparison.InvariantCultureIgnoreCase))
        {
            return GameLength.Long;
        }

        return durationName.StartsWith("very long", StringComparison.InvariantCultureIgnoreCase) ? GameLength.VeryLong : GameLength.Unknown;
    }

    private SearchRequest ConvertRequest(DownloaderSearchPayload downloaderSearchPayload, int currentPage,
        Dictionary<string, LevelTagResponse> tagLookup,
        Dictionary<string, LevelGenreResponse> genreLookup)
    {
        var searchRequest = new SearchRequest();

        searchRequest.Page = currentPage;

        searchRequest.Search = downloaderSearchPayload.AuthorName ?? string.Empty;
        if (downloaderSearchPayload.LevelName.IsNotNullOrWhiteSpace())
            searchRequest.Search = downloaderSearchPayload.LevelName ?? string.Empty;

        var rating = downloaderSearchPayload.Rating;
        if (rating > 0)
        {
            searchRequest.Ratings.Add(10 - rating + 2);
            if (rating == 10)
            {
                searchRequest.Ratings.Add(1);
            }
        }

        if (downloaderSearchPayload.GameDifficulty != null && downloaderSearchPayload.GameDifficulty != GameDifficulty.Unknown)
        {
            searchRequest.Difficulties.Add((int)downloaderSearchPayload.GameDifficulty.GetValueOrDefault());
        }

        if (downloaderSearchPayload.Duration != null && downloaderSearchPayload.Duration != GameLength.Unknown)
        {
            searchRequest.Durations.Add((int)downloaderSearchPayload.Duration.GetValueOrDefault());
        }

        if (downloaderSearchPayload.Setting.IsNotNullOrWhiteSpace())
        {
            var tokens = downloaderSearchPayload.Setting?.ToUpperInvariant().Split(",") ?? [];
            foreach (var token in tokens)
            {
                if (tagLookup != null && tagLookup.TryGetValue(token, out var targetTag))
                {
                    searchRequest.Tags.Add(targetTag.Id);
                }

                if (genreLookup != null && genreLookup.TryGetValue(token, out var targetGenre))
                {
                    searchRequest.Genres.Add(targetGenre.Id);
                }
            }
        }

        if (downloaderSearchPayload.GameEngine != null)
        {
            searchRequest.Engines.Add(_reverseEnginesMap[downloaderSearchPayload.GameEngine.GetValueOrDefault()]);
        }

        return searchRequest;
    }

    protected override async Task<ISearchResultPage> FetchPage(DownloaderSearchPayload payload, int pageNumber, CancellationToken cancellationToken)
    {
        var result = new List<IGameSearchResultMetadata>();
        var searchRequest = ConvertRequest(payload, pageNumber, _tagsMap, _genresMap);
        var dictified = RequestUtils.DictifyRequest(searchRequest);
        var response = await GetPagedResponse<LevelSummaryResponse>("levels", dictified, cancellationToken);
        ParseResultPage(response.Results, result, cancellationToken);
        return new SearchResultPage(result, response.LastPage);
    }

    public override async Task DownloadGame(IGameSearchResultMetadata metadata, Stream stream,
        IProgress<DownloadProgressInfo> downloadProgress,
        CancellationToken cancellationToken)
    {
        await HttpClient.DownloadAsync(metadata.DownloadLink!, stream, downloadProgress, cancellationToken);
    }

    public override async Task<IGameMetadata> FetchDetails(IGameSearchResultMetadata game, CancellationToken cancellationToken)
    {
        return new GameMetadataDto()
        {
            Author = game.Author,
            Description = game.Description ?? string.Empty,
            Difficulty = game.Difficulty,
            Length = game.Length,
            Setting = game.Setting,
            GameEngine = game.Engine,
            ReleaseDate = game.ReleaseDate,
            AuthorFullName = game.AuthorFullName ?? string.Empty,
            TitlePicUrl = game.TitlePic,
            TitlePic = game.TitlePic != null ? await HttpClient.GetByteArrayAsync(game.TitlePic!, cancellationToken) : Array.Empty<byte>(),
            Title = game.Title,
        };
    }

    public override DownloaderFeatures SupportedFeatures => DownloaderFeatures.Rating | DownloaderFeatures.LevelName |
                                                   DownloaderFeatures.GameEngine | DownloaderFeatures.GameLength |
                                                   DownloaderFeatures.Setting;
}