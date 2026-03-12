using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Progress;

namespace TombLauncher.Contracts.Downloaders;

public abstract class GameDownloaderBase : IGameDownloader
{
    protected readonly HttpClient _httpClient;
    
    protected GameDownloaderBase(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }
    
    public abstract string DisplayName { get; }
    public abstract string BaseUrl { get; }
    public abstract DownloaderSearchPayload DownloaderSearchPayload { get; }
    public abstract int? TotalPages { get; }
    public abstract int CurrentPage { get; }
    public abstract DownloaderFeatures SupportedFeatures { get; }
    
    public abstract Task<List<IGameSearchResultMetadata>> GetGames(DownloaderSearchPayload searchPayload, CancellationToken cancellationToken);
    public abstract Task<List<IGameSearchResultMetadata>> FetchNextPage(CancellationToken cancellationToken);
    public abstract Task<List<IGameSearchResultMetadata>> FetchPage(int pageNumber, CancellationToken cancellationToken);
    public abstract Task DownloadGame(IGameSearchResultMetadata metadata, System.IO.Stream stream, IProgress<DownloadProgressInfo> downloadProgress, CancellationToken cancellationToken);
    public abstract Task<IGameMetadata> FetchDetails(IGameSearchResultMetadata game, CancellationToken cancellationToken);
    
    public bool HasMorePages()
    {
        if (TotalPages == null) return false;
        return CurrentPage < TotalPages;
    }
}