using TombLauncher.Contracts.Enums;

namespace TombLauncher.Contracts.Downloaders;

/// <summary>
/// Aggregates all downloader capabilities via composition (HAS-A).
/// Concrete classes implement IGameSearchProvider, IGameDetailProvider and IGameInstaller
/// and expose themselves via the Search, Details and Installer properties.
/// </summary>
public interface IGameDownloader
{
    string DisplayName { get; }
    string BaseUrl { get; }
    DownloaderFeatures SupportedFeatures { get; }
    IGameSearchProvider Search { get; }
    IGameDetailProvider Details { get; }
    IGameInstaller Installer { get; }
}