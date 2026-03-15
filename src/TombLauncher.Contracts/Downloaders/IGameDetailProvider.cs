using System.Threading;
using System.Threading.Tasks;

namespace TombLauncher.Contracts.Downloaders;

public interface IGameDetailProvider
{
    string BaseUrl { get; }
    Task<IGameMetadata> FetchDetails(IGameSearchResultMetadata game, CancellationToken cancellationToken);
}
