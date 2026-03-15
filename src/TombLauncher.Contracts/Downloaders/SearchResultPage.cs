using System.Collections.Generic;

namespace TombLauncher.Contracts.Downloaders;

public record SearchResultPage(IReadOnlyList<IGameSearchResultMetadata> Results, int? TotalPages) : ISearchResultPage;
