using TombLauncher.Contracts.Downloaders;

namespace TombLauncher.Contracts.Dtos;

public class DownloaderConfigDto
{
    public string DisplayName { get; set; }
    public string BaseUrl { get; set; }
    public bool IsEnabled { get; set; }
    public int Priority { get; set; }
    public string ClassName { get; set; }
}