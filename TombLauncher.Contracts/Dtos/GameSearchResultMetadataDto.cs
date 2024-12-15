﻿using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Contracts.Dtos;

public class GameSearchResultMetadataDto : IGameSearchResultMetadata
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
    public byte[] TitlePic { get; set; }
    public string ReviewsLink { get; set; }
    public bool HasReviews => !string.IsNullOrWhiteSpace(ReviewsLink);
    public string DownloadLink { get; set; }
    public string WalkthroughLink { get; set; }
    public bool HasWalkthrough => !string.IsNullOrWhiteSpace(WalkthroughLink);
    public int? SizeInMb { get; set; }
    public double? Rating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime? ReleaseDate { get; set; }
}