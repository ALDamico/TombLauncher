using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TombLauncher.Dto;
using TombLauncher.Progress;
using TombLauncher.ViewModels;

namespace TombLauncher.Installers.Downloaders;

public interface IGameDownloader
{
    string BaseUrl { get; }
    DownloaderSearchPayload DownloaderSearchPayload { get; }
    Task<List<GameSearchResultMetadataViewModel>> GetGames(DownloaderSearchPayload searchPayload, CancellationToken cancellationToken);
    Task<List<GameSearchResultMetadataViewModel>> FetchNextPage(CancellationToken cancellationToken);
    Task DownloadGame(IGameSearchResultMetadata metadata, Stream stream, IProgress<DownloadProgressInfo> downloadProgress, CancellationToken cancellationToken);
    Task<GameMetadataDto> FetchDetails(IGameSearchResultMetadata game, CancellationToken cancellationToken);
    bool HasMorePages();
    int TotalPages { get; }
    int CurrentPage { get; }
}