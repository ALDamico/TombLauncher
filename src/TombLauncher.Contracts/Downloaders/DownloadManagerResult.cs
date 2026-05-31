using System.Collections.Concurrent;

namespace TombLauncher.Contracts.Downloaders;

public class DownloadManagerResult
{
    public List<IMergedGameSearchResultMetadata> Results { get; init; } = [];
    public int? MaxTotalPages { get; set; }
    public ConcurrentBag<string> FailedDownloaders { get; set; } = [];
    public IGameSearchResultMetadata? Details { get; set; }
}