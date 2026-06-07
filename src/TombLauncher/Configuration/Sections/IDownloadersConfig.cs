using System.Collections.Generic;
using TombLauncher.Contracts.Settings;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Configuration.Sections;

public interface IDownloadersConfig
{
    List<DownloaderConfiguration>? Sources { get; }
    string? UnzipFallbackMethod { get; }
}
