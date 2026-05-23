namespace TombLauncher.Contracts.Downloaders;

public record SearchResultPage : ISearchResultPage
{
    public SearchResultPage(IReadOnlyList<IGameSearchResultMetadata> results, int? totalPages)
    {
        Results = results;
        TotalPages = totalPages;
    }
    
    public IReadOnlyList<IGameSearchResultMetadata> Results { get; init; } = [];
    public int? TotalPages { get; init; }

    public static ISearchResultPage EmptyPage => new SearchResultPage([], 0);
}
