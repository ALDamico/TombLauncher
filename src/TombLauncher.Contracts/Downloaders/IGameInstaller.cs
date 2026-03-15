using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TombLauncher.Contracts.Progress;

namespace TombLauncher.Contracts.Downloaders;

public interface IGameInstaller
{
    string BaseUrl { get; }
    Task DownloadGame(IGameSearchResultMetadata metadata, Stream stream,
        IProgress<DownloadProgressInfo> downloadProgress, CancellationToken cancellationToken);
}
