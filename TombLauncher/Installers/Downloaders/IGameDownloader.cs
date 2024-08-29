﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TombLauncher.Dto;
using TombLauncher.Progress;
using TombLauncher.ViewModels;

namespace TombLauncher.Installers.Downloaders;

public interface IGameDownloader
{
    string BaseUrl { get; }
    Task<List<GameSearchResultMetadataViewModel>> GetGames(DownloaderSearchPayload searchPayload, CancellationToken cancellationToken);
    Task<GameMetadataDto> DownloadGame(GameSearchResultMetadataViewModel metadata, IProgress<DownloadProgressInfo> downloadProgress);
}