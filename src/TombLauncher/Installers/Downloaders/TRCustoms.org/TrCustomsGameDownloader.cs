using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Utils;
using TombLauncher.Extensions;
using TombLauncher.Installers.Downloaders.TRCustoms.org.Requests;
using TombLauncher.Installers.Downloaders.TRCustoms.org.Responses;
using TombLauncher.Installers.Downloaders.TRCustoms.org.Utils;

namespace TombLauncher.Installers.Downloaders.TRCustoms.org;

public class TrCustomsGameDownloader : IGameDownloader
{
    public string DisplayName => "TRCustoms.org";
    public string BaseUrl => "https://trcustoms.org/";
    public DownloaderSearchPayload DownloaderSearchPayload { get; private set; } = new();

    public TrCustomsGameDownloader()
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(BaseUrl),
            DefaultRequestHeaders =
            {
                Accept =
                {
                    new MediaTypeWithQualityHeaderValue("application/json")
                }
            }
        };
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

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    private Dictionary<int, GameEngine> _enginesMap = new();
    private Dictionary<GameEngine, int> _reverseEnginesMap = new();
    private Dictionary<string, LevelTagResponse> _tagsMap = new();
    private Dictionary<string, LevelGenreResponse> _genresMap = new();
    private readonly Dictionary<string, int> _ratingMap;

    public async Task<List<IGameSearchResultMetadata>> GetGames(DownloaderSearchPayload searchPayload,
        CancellationToken cancellationToken)
    {
        DownloaderSearchPayload = searchPayload;
        cancellationToken.ThrowIfCancellationRequested();

        var engineMapTask = FetchSupportedEngines(cancellationToken);
        var genreMapTask = FetchGenres(cancellationToken);
        var tagMapTask = FetchTags(cancellationToken);

        await Task.WhenAll(engineMapTask, genreMapTask, tagMapTask);
        _enginesMap = engineMapTask.Result;
        _reverseEnginesMap = _enginesMap.ToDictionary(k => k.Value, k => k.Key);
        _tagsMap = tagMapTask.Result;
        _genresMap = genreMapTask.Result;

        CurrentPage = 0;
        return await FetchNextPage(cancellationToken);
    }

    private async Task<TrCustomsPagedResponse<T>> GetPagedResponse<T>(string endpoint, IEnumerable<KeyValuePair<string, string?>>? trCustomsRequest = null,
        CancellationToken cancellationToken = default)
    {
        var relativeUri = new UriBuilder(_httpClient.BaseAddress!);
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
        using var response = await _httpClient.SendAsync(request, cancellationToken);
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
        return tags?.Results?.ToDictionary(t => t.Name?.ToUpperInvariant() ?? string.Empty, t => t) ?? new Dictionary<string, LevelTagResponse>();
    }

    private async Task<Dictionary<string, LevelGenreResponse>> FetchGenres(CancellationToken cancellationToken)
    {
        var request = new TrCustomsBaseRequest()
        {
            PageSize = 50
        };
        var dictified = RequestUtils.DictifyRequest(request);
        var genres = await GetPagedResponse<LevelGenreResponse>("level_genres", dictified, cancellationToken);
        return genres?.Results?.ToDictionary(g => g.Name?.ToUpperInvariant() ?? string.Empty, g => g) ?? new Dictionary<string, LevelGenreResponse>();
    }

    private async Task<Dictionary<int, GameEngine>> FetchSupportedEngines(CancellationToken cancellationToken)
    {
        var engines = await GetPagedResponse<LevelEngineResponse>("level_engines", cancellationToken: cancellationToken);

        var dict = new Dictionary<int, GameEngine>();
        if (engines == null)
            return dict;

        foreach (var response in engines.Results)
        {
            GameEngine engine = GameEngine.Unknown;
            switch (response.Name)
            {
                case "TR1":
                    engine = GameEngine.TombRaider1;
                    break;
                case "TR2":
                    engine = GameEngine.TombRaider2;
                    break;
                case "TR3":
                    engine = GameEngine.TombRaider3;
                    break;
                case "TR4":
                    engine = GameEngine.TombRaider4;
                    break;
                case "TR5":
                    engine = GameEngine.TombRaider5;
                    break;
                case "TEN":
                    engine = GameEngine.Ten;
                    break;
            }

            dict[response.Id] = engine;
        }

        return dict;
    }

    public async Task<List<IGameSearchResultMetadata>> FetchNextPage(CancellationToken cancellationToken)
    {
        if (CurrentPage > TotalPages) return new List<IGameSearchResultMetadata>();
        CurrentPage++;
        var result = new List<IGameSearchResultMetadata>();
        var searchRequest = ConvertRequest(DownloaderSearchPayload, CurrentPage, _tagsMap, _genresMap);
        var dictified = RequestUtils.DictifyRequest(searchRequest);
        var response = await GetPagedResponse<LevelSummaryResponse>("levels", dictified, cancellationToken);
        if (TotalPages == null)
            TotalPages = response.LastPage;
        ParseResultPage(response.Results, result, cancellationToken);
        return result;
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

            var tags = summary.Tags?.Select(t => t.Name) ?? [];
            string? settingString = tags.Any() ? string.Join(", ", tags) : null;

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
                ReviewsLink = $"levels/{summary.Id}/reviews",
                DetailsLink = $"levels/{summary.Id}",
                DownloadLink = summary.LastFile?.Url ?? string.Empty
            };
            result.Add(metadata);
        }
    }

    private double? ParseRating(RatingClassResponse? ratingClass)
    {
        double? rating = null;
        if (ratingClass != null && ratingClass.Name != null && _ratingMap.ContainsKey(ratingClass.Name))
        {
            rating = _ratingMap[ratingClass.Name];
        }

        return rating;
    }

    private GameDifficulty ParseDifficulty(LevelDifficultyResponse difficultyResponse)
    {
        var difficulty = GameDifficulty.Unknown;
        if (difficultyResponse != null)
        {
            difficulty = (GameDifficulty)difficultyResponse.Id;
        }

        return difficulty;
    }

    private GameEngine ParseGameEngine(LevelEngineResponse engine)
    {
        var gameEngine = GameEngine.Unknown;
        if (engine != null)
        {
            gameEngine = _enginesMap[engine.Id];
        }

        return gameEngine;
    }

    private GameLength ParseDuration(LevelDurationResponse levelDurationResponse)
    {
        if (levelDurationResponse == null)
            return GameLength.Unknown;

        var durationName = levelDurationResponse.Name;
        if (durationName == null) return GameLength.Unknown;

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

        if (durationName.StartsWith("very long", StringComparison.InvariantCultureIgnoreCase))
        {
            return GameLength.VeryLong;
        }

        return GameLength.Unknown;
    }

    private SearchRequest ConvertRequest(DownloaderSearchPayload downloaderSearchPayload, int currentPage,
        Dictionary<string, LevelTagResponse> tagLookup,
        Dictionary<string, LevelGenreResponse> genreLookup)
    {
        var searchRequest = new SearchRequest();

        searchRequest.Page = currentPage;

        searchRequest.Search = DownloaderSearchPayload.AuthorName ?? string.Empty;
        if (downloaderSearchPayload.LevelName != null)
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

    public async Task<List<IGameSearchResultMetadata>> FetchPage(int pageNumber, CancellationToken cancellationToken)
    {
        var result = new List<IGameSearchResultMetadata>();
        var searchRequest = ConvertRequest(DownloaderSearchPayload, pageNumber, _tagsMap, _genresMap);
        var dictified = RequestUtils.DictifyRequest(searchRequest);
        var response = await GetPagedResponse<LevelSummaryResponse>("levels", dictified, cancellationToken);
        ParseResultPage(response.Results, result, cancellationToken);
        return result;
    }

    public async Task DownloadGame(IGameSearchResultMetadata metadata, Stream stream,
        IProgress<DownloadProgressInfo> downloadProgress,
        CancellationToken cancellationToken)
    {
        await _httpClient.DownloadAsync(metadata.DownloadLink, stream, downloadProgress, cancellationToken);
    }

    public async Task<IGameMetadata> FetchDetails(IGameSearchResultMetadata game, CancellationToken cancellationToken)
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
            TitlePic = game.TitlePic != null ? await _httpClient.GetByteArrayAsync(game.TitlePic!, cancellationToken) : Array.Empty<byte>(),
            Title = game.Title ?? string.Empty,
        };
    }

    public bool HasMorePages()
    {
        if (TotalPages == null) return false;
        return CurrentPage < TotalPages;
    }

    public int? TotalPages { get; private set; }
    public int CurrentPage { get; private set; }

    public DownloaderFeatures SupportedFeatures => DownloaderFeatures.Rating | DownloaderFeatures.LevelName |
                                                   DownloaderFeatures.GameEngine | DownloaderFeatures.GameLength |
                                                   DownloaderFeatures.Setting;
}