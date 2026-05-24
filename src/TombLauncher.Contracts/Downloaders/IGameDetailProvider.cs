namespace TombLauncher.Contracts.Downloaders;

public interface IGameDetailProvider
{
    string BaseUrl { get; }
    Task<IGameMetadata> FetchDetails(IGameSearchResultMetadata game, CancellationToken cancellationToken);
    Task<IGameSearchResultMetadata?> FetchDetails(string detailId, CancellationToken cancellationToken);
}
