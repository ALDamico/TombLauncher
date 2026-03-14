using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Installers.Downloaders;

public abstract class GameDownloaderBase : IGameDownloader
{
    protected GameDownloaderBase(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(GetType().Name);
    }

    protected readonly HttpClient _httpClient;

    public abstract string DisplayName { get; }
    public abstract string BaseUrl { get; }
    public abstract DownloaderFeatures SupportedFeatures { get; }

    public DownloaderSearchPayload DownloaderSearchPayload { get; protected set; } = new();
    public int? TotalPages { get; protected set; }
    public int CurrentPage { get; protected set; }

    public virtual async Task<List<IGameSearchResultMetadata>> GetGames(
        DownloaderSearchPayload searchPayload, CancellationToken cancellationToken)
    {
        DownloaderSearchPayload = searchPayload;
        cancellationToken.ThrowIfCancellationRequested();
        CurrentPage = 0;
        TotalPages = null;
        return await FetchNextPage(cancellationToken);
    }

    public virtual async Task<List<IGameSearchResultMetadata>> FetchNextPage(CancellationToken cancellationToken)
    {
        if (CurrentPage >= TotalPages) return new List<IGameSearchResultMetadata>();
        CurrentPage++;
        return await FetchPage(CurrentPage, cancellationToken);
    }

    public bool HasMorePages()
    {
        if (TotalPages == null) return false;
        return CurrentPage < TotalPages;
    }

    public abstract Task<List<IGameSearchResultMetadata>> FetchPage(int pageNumber, CancellationToken cancellationToken);
    public abstract Task<IGameMetadata> FetchDetails(IGameSearchResultMetadata game, CancellationToken cancellationToken);
    public abstract Task DownloadGame(IGameSearchResultMetadata metadata, Stream stream,
        IProgress<DownloadProgressInfo> downloadProgress, CancellationToken cancellationToken);
}
