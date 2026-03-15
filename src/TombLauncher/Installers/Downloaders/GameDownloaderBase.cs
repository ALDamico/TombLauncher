using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Progress;

namespace TombLauncher.Installers.Downloaders;

public abstract class GameDownloaderBase : IGameDownloader, IGameSearchProvider, IGameDetailProvider, IGameInstaller
{
    protected GameDownloaderBase(IHttpClientFactory httpClientFactory)
    {
        HttpClient = httpClientFactory.CreateClient(GetType().Name);
    }

    protected readonly HttpClient HttpClient;

    public abstract string DisplayName { get; }
    public abstract string BaseUrl { get; }
    public abstract DownloaderFeatures SupportedFeatures { get; }

    // IGameDownloader composition via self-reference
    IGameSearchProvider IGameDownloader.Search => this;
    IGameDetailProvider IGameDownloader.Details => this;
    IGameInstaller IGameDownloader.Installer => this;

    // IGameSearchProvider — stateless: all state passed as parameters
    public virtual Task<ISearchResultPage> GetGames(DownloaderSearchPayload payload, int page, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return FetchPage(payload, page, cancellationToken);
    }

    protected abstract Task<ISearchResultPage> FetchPage(DownloaderSearchPayload payload, int pageNumber, CancellationToken cancellationToken);

    // IGameDetailProvider
    public abstract Task<IGameMetadata> FetchDetails(IGameSearchResultMetadata game, CancellationToken cancellationToken);

    // IGameInstaller
    public abstract Task DownloadGame(IGameSearchResultMetadata metadata, Stream stream,
        IProgress<DownloadProgressInfo> downloadProgress, CancellationToken cancellationToken);
}
