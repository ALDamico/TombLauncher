using System.Collections.Generic;
using TombLauncher.Contracts.Settings;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Configuration.Sections;

public class DownloadersConfig : IDownloadersConfig
{
    public List<DownloaderConfiguration>? Sources { get; set; }
    public string? UnzipFallbackMethod { get; set; }
}
