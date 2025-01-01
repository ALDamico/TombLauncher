using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Progress;

namespace TombLauncher.Contracts.Downloaders;

public interface IGameDownloader
{
    string DisplayName { get; }
    string BaseUrl { get; }
    DownloaderSearchPayload DownloaderSearchPayload { get; }
    Task<List<IGameSearchResultMetadata>> GetGames(DownloaderSearchPayload searchPayload, CancellationToken cancellationToken);
    Task<List<IGameSearchResultMetadata>> FetchNextPage(CancellationToken cancellationToken);
    Task DownloadGame(IGameSearchResultMetadata metadata, Stream stream, IProgress<DownloadProgressInfo> downloadProgress, CancellationToken cancellationToken);
    Task<IGameMetadata> FetchDetails(IGameSearchResultMetadata game, CancellationToken cancellationToken);
    bool HasMorePages();
    int? TotalPages { get; }
    int CurrentPage { get; }
    DownloaderFeatures SupportedFeatures { get; }
}