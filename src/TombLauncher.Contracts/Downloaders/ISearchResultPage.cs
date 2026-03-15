using System.Collections.Generic;

namespace TombLauncher.Contracts.Downloaders;

public interface ISearchResultPage
{
    IReadOnlyList<IGameSearchResultMetadata> Results { get; }
    int? TotalPages { get; }
}
