namespace TombLauncher.Contracts.Downloaders;

public interface IMultiSourceSearchResultMetadata : IGameSearchResultMetadata
{
    public List<IGameSearchResultMetadata> Sources { get; set; }
}