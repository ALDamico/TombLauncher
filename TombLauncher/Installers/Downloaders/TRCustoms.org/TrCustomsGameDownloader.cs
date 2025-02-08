using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Progress;

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
            BaseAddress = new Uri(BaseUrl)
        };
        _jsonSerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };
    }

    private HttpClient _httpClient;

    public async Task<List<IGameSearchResultMetadata>> GetGames(DownloaderSearchPayload searchPayload,
        CancellationToken cancellationToken)
    {
        DownloaderSearchPayload = searchPayload;
        cancellationToken.ThrowIfCancellationRequested();

        var engineMap = await FetchSupportedEngines(cancellationToken);
        CurrentPage = 0;
        return null;
    }

    private JsonSerializerSettings _jsonSerializerSettings;

    private async Task<Dictionary<GameEngine, LevelEngineResponse>> FetchSupportedEngines(
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(_httpClient.BaseAddress, "/api/level_engines/?format=json"),
            Headers =
            {
                { "User-Agent", "insomnia/10.3.0" },
            },
        };
        TrCustomsPagedResponse<LevelEngineResponse> engines;
        using (var response = await _httpClient.SendAsync(request, cancellationToken))
        {
            response.EnsureSuccessStatusCode();

            engines = JsonConvert.DeserializeObject<TrCustomsPagedResponse<LevelEngineResponse>>(
                await response.Content.ReadAsStringAsync(cancellationToken), _jsonSerializerSettings);
        }

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

    public Task<List<IGameSearchResultMetadata>> FetchNextPage(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public int? TotalPages { get; private set; }
    public int CurrentPage { get; private set; }

    public DownloaderFeatures SupportedFeatures => DownloaderFeatures.Rating | DownloaderFeatures.LevelName |
                                                   DownloaderFeatures.GameEngine | DownloaderFeatures.GameLength |
                                                   DownloaderFeatures.Setting;
}