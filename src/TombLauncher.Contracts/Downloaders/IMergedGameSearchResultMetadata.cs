namespace TombLauncher.Contracts.Downloaders;

public interface IMergedGameSearchResultMetadata : IGameSearchResultMetadata
{
    public HashSet<IGameSearchResultMetadata> Sources { get; set; }
}