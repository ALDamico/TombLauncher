using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;

namespace TombLauncher.Core.Dtos;

public class MergedGameSearchResultDto : IMergedGameSearchResultMetadata
{
    public string? Author { get; set; }
    public string? AuthorFullName { get; set; }
    public string? Description { get; set; }
    public string Title { get; set; } = string.Empty;
    public GameDifficulty Difficulty { get; set; }
    public GameLength Length { get; set; }
    public string? Setting { get; set; }
    public GameEngine Engine { get; set; }
    public string? DetailsLink { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string SourceSiteDisplayName { get; set; } = string.Empty;
    public string? TitlePic { get; set; }
    public string? ReviewsLink { get; set; }
    public bool HasReviews => ReviewsLink.IsNotNullOrWhiteSpace();
    public string? DownloadLink { get; set; }
    public string? WalkthroughLink { get; set; }
    public bool HasWalkthrough => WalkthroughLink.IsNotNullOrWhiteSpace();
    public int? SizeInMb { get; set; }
    public double? Rating { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public HashSet<IGameSearchResultMetadata> Sources { get; set; } = new HashSet<IGameSearchResultMetadata>();
    public int ReviewCount => Sources.Sum(s => s.ReviewCount);
}