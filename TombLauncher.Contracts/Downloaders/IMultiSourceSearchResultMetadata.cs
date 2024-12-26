namespace TombLauncher.Contracts.Downloaders;

public interface IMultiSourceSearchResultMetadata : IGameSearchResultMetadata
{
    public HashSet<IGameSearchResultMetadata> Sources { get; set; }
}