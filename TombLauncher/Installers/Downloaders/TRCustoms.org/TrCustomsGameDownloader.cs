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
using TombLauncher.Core.Extensions;
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
    }

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    private Dictionary<GameEngine, LevelEngineResponse> _enginesMap;
    private Dictionary<string, LevelTagResponse> _tagsMap;
    private Dictionary<string, LevelGenreResponse> _genresMap;

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

    

    private async Task<TrCustomsPagedResponse<T>> GetPagedResponse<T>(string endpoint, TrCustomsBaseRequest trCustomsRequest = null,
        CancellationToken cancellationToken = default)
    {
        var dictionarifiedRequest = RequestUtils.DictifyRequest(trCustomsRequest);
        var relativeUri = new UriBuilder(_httpClient.BaseAddress);
        relativeUri.Path = $"api/{endpoint}/";
        var completeUri = QueryHelpers.AddQueryString(relativeUri.Uri.ToString(), dictionarifiedRequest);
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

        var tags = await GetPagedResponse<LevelTagResponse>("level_tags", request, cancellationToken);
        return tags.Results.ToDictionary(t => t.Name.ToUpperInvariant(), t => t);
    }

    private async Task<Dictionary<string, LevelGenreResponse>> FetchGenres(CancellationToken cancellationToken)
    {
        var request = new TrCustomsBaseRequest()
        {
            PageSize = 50
        };
        var genres = await GetPagedResponse<LevelGenreResponse>("level_genres", request, cancellationToken);
        return genres.Results.ToDictionary(g => g.Name.ToUpperInvariant(), g => g);
    }

    private async Task<Dictionary<GameEngine, LevelEngineResponse>> FetchSupportedEngines(CancellationToken cancellationToken)
    {
        var engines = await GetPagedResponse<LevelEngineResponse>("level_engines", cancellationToken:cancellationToken);

        var dict = new Dictionary<GameEngine, LevelEngineResponse>();
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

            dict[engine] = response;
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
        var response = await GetPagedResponse<LevelSummaryResponse>("levels", searchRequest, cancellationToken);
        return result;
    }

    private SearchRequest ConvertRequest(DownloaderSearchPayload downloaderSearchPayload, int currentPage, Dictionary<string, LevelTagResponse> tagLookup, Dictionary<string, LevelGenreResponse> genreLookup)
    {
        var searchRequest = new SearchRequest();

        searchRequest.Page = currentPage;
        
        searchRequest.Search = DownloaderSearchPayload.LevelName;
        if (downloaderSearchPayload.LevelName != null)
            searchRequest.Search = downloaderSearchPayload.LevelName;

        var rating = downloaderSearchPayload.Rating;
        searchRequest.Ratings.Add(rating);
        if (rating == 10)
        {
            searchRequest.Ratings.Add(11);
        }

        if (downloaderSearchPayload.GameDifficulty != GameDifficulty.Unknown)
        {
            searchRequest.Difficulties.Add((int)downloaderSearchPayload.GameDifficulty.GetValueOrDefault());    
        }

        if (downloaderSearchPayload.Duration != GameLength.Unknown)
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

    public Task<List<IGameSearchResultMetadata>> FetchPage(int pageNumber, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DownloadGame(IGameSearchResultMetadata metadata, Stream stream,
        IProgress<DownloadProgressInfo> downloadProgress,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IGameMetadata> FetchDetails(IGameSearchResultMetadata game, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
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