using TombLauncher.Contracts.Dtos;
using TombLauncher.Contracts.Progress;

namespace TombLauncher.Contracts.Downloaders;

public interface IGameDownloadManager
{
    List<IGameDownloader> Downloaders { get; }
    Task<List<IMultiSourceSearchResultMetadata>> GetGames(DownloaderSearchPayload searchPayload);
    Task<List<IGameSearchResultMetadata>> FetchNextPage();
    Task<GameMetadataDto> FetchDetails(IGameSearchResultMetadata game);
    void CancelCurrentAction();
    bool HasMoreResults();

    void Merge(ICollection<IMultiSourceSearchResultMetadata> fullList,
        ICollection<IGameSearchResultMetadata> newElements);

    Task<string> DownloadGame(IGameSearchResultMetadata metadata, IProgress<DownloadProgressInfo> downloadProgress, CancellationToken cancellationToken);
}