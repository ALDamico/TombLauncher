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
    public DownloaderSearchPayload DownloaderSearchPayload { get; private set; }

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

    private Dictionary<int, GameEngine> _enginesMap;
    private Dictionary<string, LevelTagResponse> _tagsMap;
    private Dictionary<string, LevelGenreResponse> _genresMap;
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
        _tagsMap = tagMapTask.Result;
        _genresMap = genreMapTask.Result;
        
        CurrentPage = 0;
        return await FetchNextPage(cancellationToken);
    }

    private async Task<TrCustomsPagedResponse<T>> GetPagedResponse<T>(string endpoint, IEnumerable<KeyValuePair<string, string>> trCustomsRequest = null,
        CancellationToken cancellationToken = default)
    {
        var relativeUri = new UriBuilder(_httpClient.BaseAddress);
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
// TODO Crashes because LastFile is an object, not a string
        return pagedResponse;
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
        return tags.Results.ToDictionary(t => t.Name.ToUpperInvariant(), t => t);
    }

    private async Task<Dictionary<string, LevelGenreResponse>> FetchGenres(CancellationToken cancellationToken)
    {
        var request = new TrCustomsBaseRequest()
        {
            PageSize = 50
        };
        var dictified = RequestUtils.DictifyRequest(request);
        var genres = await GetPagedResponse<LevelGenreResponse>("level_genres", dictified, cancellationToken);
        return genres.Results.ToDictionary(g => g.Name.ToUpperInvariant(), g => g);
    }

    private async Task<Dictionary<int, GameEngine>> FetchSupportedEngines(CancellationToken cancellationToken)
    {
        var engines = await GetPagedResponse<LevelEngineResponse>("level_engines", cancellationToken:cancellationToken);

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
        ParseResultPage(response.Results, result, cancellationToken);
        return result;
    }

    private void ParseResultPage(List<LevelSummaryResponse> levelSummaries, List<IGameSearchResultMetadata> result,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (var summary in levelSummaries)
        {
            string authors = null;
            if (summary.Authors.IsNotNullOrEmpty())
            {
                authors = string.Join(", ", summary.Authors.Select(a => a.Username));
            }

            var tags = summary.Tags.Select(t => t.Name);

            string settingString =  string.Join(", ", tags);

            var rating = ParseRating(summary.RatingClass);

            var difficulty = ParseDifficulty(summary.Difficulty);

            var gameEngine = ParseGameEngine(summary.Engine);
            
            var metadata = new GameSearchResultMetadataDto()
            {
                BaseUrl = BaseUrl,
                SourceSiteDisplayName = DisplayName,
                Author = authors,
                Rating = rating,
                Description = summary.Description,
                Difficulty = difficulty,
                Engine = gameEngine,
                Length = ParseDuration(summary.Duration),
                Setting = settingString,
                Title = summary.Name,
                ReleaseDate = summary.Created,
                TitlePic = summary.Cover.Url,
                SizeInMb = summary.LastFile.Size,
                DownloadLink = summary.LastFile.Url,
                ReviewCount = summary.ReviewCount,
                ReviewsLink = $"levels/{summary.Id}/reviews",
                DetailsLink = $"levels/{summary.Id}"
            };
            result.Add(metadata);
        }
    }

    private double? ParseRating(RatingClassResponse ratingClass)
    {
        double? rating = null;
        if (ratingClass != null)
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
        
        searchRequest.Search = DownloaderSearchPayload.AuthorName;
        if (downloaderSearchPayload.LevelName != null)
            searchRequest.Search = downloaderSearchPayload.LevelName;

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
            var tokens = downloaderSearchPayload.Setting.ToUpperInvariant().Split(",");
            foreach (var token in tokens)
            {
                if (tagLookup.TryGetValue(token, out var targetTag))
                {
                    searchRequest.Tags.Add(targetTag.Id);
                }

                if (genreLookup.TryGetValue(token, out var targetGenre))
                {
                    searchRequest.Genres.Add(targetGenre.Id);
                }
            }
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
            Description = game.Description,
            Difficulty = game.Difficulty,
            Length = game.Length,
            Setting = game.Setting,
            GameEngine = game.Engine,
            ReleaseDate = game.ReleaseDate,
            AuthorFullName = game.AuthorFullName,
            TitlePic = await _httpClient.GetByteArrayAsync(game.TitlePic, cancellationToken),
            Title = game.Title,
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