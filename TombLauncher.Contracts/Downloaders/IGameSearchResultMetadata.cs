using TombLauncher.Contracts.Enums;

namespace TombLauncher.Contracts.Downloaders;

public interface IGameSearchResultMetadata
{
    string Author { get; set; }
    string AuthorFullName { get; set; }
    string Title { get; set; }
    GameDifficulty Difficulty { get; set; }
    GameLength Length { get; set; }
    string Setting { get; set; }
    GameEngine Engine { get; set; }
    string DetailsLink { get; set; }
    string BaseUrl { get; set; }
    string SourceSiteDisplayName { get; set; }
    public byte[] TitlePic { get; set; }
    string DownloadLink { get; set; }
    int? SizeInMb { get; set; }
    double? Rating { get; set; }
    int ReviewCount { get; }
    DateTime? ReleaseDate { get; set; }
    string ReviewsLink { get; set; }
    bool HasReviews { get; }
    string WalkthroughLink { get; set; }
    bool HasWalkthrough { get; }
}