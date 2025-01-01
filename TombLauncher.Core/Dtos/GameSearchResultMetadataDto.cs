using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;

namespace TombLauncher.Core.Dtos;

public class GameSearchResultMetadataDto : IGameSearchResultMetadata, IEquatable<GameSearchResultMetadataDto>
{
    public string Author { get; set; }
    public string AuthorFullName { get; set; }
    public string Title { get; set; }
    public GameDifficulty Difficulty { get; set; }
    public GameLength Length { get; set; }
    public string Setting { get; set; }
    public GameEngine Engine { get; set; }
    public string DetailsLink { get; set; }
    public string BaseUrl { get; set; }
    public string SourceSiteDisplayName { get; set; }
    public string TitlePic { get; set; }
    public string ReviewsLink { get; set; }
    public bool HasReviews => ReviewsLink.IsNotNullOrWhiteSpace();
    public string DownloadLink { get; set; }
    public string WalkthroughLink { get; set; }
    public bool HasWalkthrough => WalkthroughLink.IsNotNullOrWhiteSpace();
    public int? SizeInMb { get; set; }
    public double? Rating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime? ReleaseDate { get; set; }

    public bool Equals(GameSearchResultMetadataDto other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Author == other.Author && AuthorFullName == other.AuthorFullName && Title == other.Title &&
               Difficulty == other.Difficulty && Length == other.Length && Setting == other.Setting &&
               Engine == other.Engine && DetailsLink == other.DetailsLink && BaseUrl == other.BaseUrl &&
               SourceSiteDisplayName == other.SourceSiteDisplayName && Equals(TitlePic, other.TitlePic) &&
               ReviewsLink == other.ReviewsLink && DownloadLink == other.DownloadLink &&
               WalkthroughLink == other.WalkthroughLink && SizeInMb == other.SizeInMb &&
               Nullable.Equals(Rating, other.Rating) && ReviewCount == other.ReviewCount &&
               Nullable.Equals(ReleaseDate, other.ReleaseDate);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((GameSearchResultMetadataDto)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Author);
        hashCode.Add(AuthorFullName);
        hashCode.Add(Title);
        hashCode.Add((int)Difficulty);
        hashCode.Add((int)Length);
        hashCode.Add(Setting);
        hashCode.Add((int)Engine);
        hashCode.Add(DetailsLink);
        hashCode.Add(BaseUrl);
        hashCode.Add(SourceSiteDisplayName);
        hashCode.Add(TitlePic);
        hashCode.Add(ReviewsLink);
        hashCode.Add(DownloadLink);
        hashCode.Add(WalkthroughLink);
        hashCode.Add(SizeInMb);
        hashCode.Add(Rating);
        hashCode.Add(ReviewCount);
        hashCode.Add(ReleaseDate);
        return hashCode.ToHashCode();
    }
}