namespace TombLauncher.ViewModels;

public interface IGameSearchResultMetadata
{
    /// <inheritdoc cref="GameSearchResultMetadataViewModel._author"/>
    string Author
    {
        get;
        [global::System.Diagnostics.CodeAnalysis.MemberNotNull("_author")]
        set;
    }

    /// <inheritdoc cref="GameSearchResultMetadataViewModel._authorFullName"/>
    string AuthorFullName
    {
        get;
        [global::System.Diagnostics.CodeAnalysis.MemberNotNull("_authorFullName")]
        set;
    }

    /// <inheritdoc cref="GameSearchResultMetadataViewModel._title"/>
    string Title
    {
        get;
        [global::System.Diagnostics.CodeAnalysis.MemberNotNull("_title")]
        set;
    }

    /// <inheritdoc cref="GameSearchResultMetadataViewModel._difficulty"/>
    global::TombLauncher.Data.Models.GameDifficulty Difficulty { get; set; }

    /// <inheritdoc cref="GameSearchResultMetadataViewModel._length"/>
    global::TombLauncher.Data.Models.GameLength Length { get; set; }

    /// <inheritdoc cref="GameSearchResultMetadataViewModel._setting"/>
    string Setting
    {
        get;
        [global::System.Diagnostics.CodeAnalysis.MemberNotNull("_setting")]
        set;
    }

    /// <inheritdoc cref="GameSearchResultMetadataViewModel._engine"/>
    global::TombLauncher.Data.Models.GameEngine Engine { get; set; }

    /// <inheritdoc cref="GameSearchResultMetadataViewModel._detailsLink"/>
    string DetailsLink
    {
        get;
        [global::System.Diagnostics.CodeAnalysis.MemberNotNull("_detailsLink")]
        set;
    }

    /// <inheritdoc cref="GameSearchResultMetadataViewModel._baseUrl"/>
    string BaseUrl
    {
        get;
        [global::System.Diagnostics.CodeAnalysis.MemberNotNull("_baseUrl")]
        set;
    }

    /// <inheritdoc cref="GameSearchResultMetadataViewModel._titlePic"/>
    global::Avalonia.Media.Imaging.Bitmap TitlePic
    {
        get;
        [global::System.Diagnostics.CodeAnalysis.MemberNotNull("_titlePic")]
        set;
    }

    /// <inheritdoc cref="GameSearchResultMetadataViewModel._downloadLink"/>
    string DownloadLink
    {
        get;
        [global::System.Diagnostics.CodeAnalysis.MemberNotNull("_downloadLink")]
        set;
    }

    /// <inheritdoc cref="GameSearchResultMetadataViewModel._sizeInMb"/>
    int? SizeInMb { get; set; }

    /// <inheritdoc cref="GameSearchResultMetadataViewModel._rating"/>
    double? Rating { get; set; }

    /// <inheritdoc cref="GameSearchResultMetadataViewModel._reviewCount"/>
    int ReviewCount { get; }

    /// <inheritdoc cref="GameSearchResultMetadataViewModel._releaseDate"/>
    global::System.DateTime? ReleaseDate { get; set; }

    string ReviewsLink { get; set; }
    bool HasReviews { get; }
    string WalkthroughLink { get; set; }
    bool HasWalkthrough { get; }
}