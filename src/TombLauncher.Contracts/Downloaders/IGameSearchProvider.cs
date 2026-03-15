using System.Threading;
using System.Threading.Tasks;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Contracts.Downloaders;

public interface IGameSearchProvider
{
    string BaseUrl { get; }
    string DisplayName { get; }
    DownloaderFeatures SupportedFeatures { get; }
    Task<ISearchResultPage> GetGames(DownloaderSearchPayload payload, int page, CancellationToken cancellationToken);
}
