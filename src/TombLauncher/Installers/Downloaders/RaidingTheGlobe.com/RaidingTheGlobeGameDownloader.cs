using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Utils;

namespace TombLauncher.Installers.Downloaders.RaidingTheGlobe.com;

public partial class RaidingTheGlobeGameDownloader : GameDownloaderBase
{
    private readonly Dictionary<string, GameEngine> _enginesLookup = new()
    {
        { "Lara Croft Tomb Raider: The Scion of Qualopec", GameEngine.TombRaider4 },
        { "Lara Croft Tomb Raider: Afterlife", GameEngine.TombRaider4 }
    };
    
    public override string DisplayName => "Raiding the Globe";
    public override string BaseUrl => "https://www.raidingtheglobe.com";
    public override DownloaderFeatures SupportedFeatures => DownloaderFeatures.LevelName;
    public override Regex DetailsPageRegex => DetailsRegex();

    [GeneratedRegex(@"raidingtheglobe.com\/downloads\/custom-games-tomb-raider-level-editor\/\d+")]
    private static partial Regex DetailsRegex();

    protected override async Task<ISearchResultPage> FetchPage(DownloaderSearchPayload payload, int pageNumber, CancellationToken cancellationToken)
    {
        var uriBuilder = new UriBuilder(new Uri(BaseUrl));
        uriBuilder.Path = "/downloads/custom-games-tomb-raider-level-editor";

        var page = await HttpClient.GetStringAsync(uriBuilder.Uri, cancellationToken);

        var htmlDocument = await AppUtils.OpenDocumentFromContent(page, cancellationToken);

        var allLevels = new List<IGameSearchResultMetadata>();

        var levels = htmlDocument.Body!.QuerySelectorAll(".docman_document");
        foreach (var level in levels)
        {
            var levelTitle = GetLevelTitle(level);
            if (levelTitle == null)
            {
                Logger.LogWarning("Unable to fetch level title from Raiding the Globe!");
                continue;
            }
            
            var levelMetadata = new GameSearchResultMetadataDto()
            {
                Author = "Raiding the Globe staff",
                Engine = _enginesLookup.GetValueOrDefault(levelTitle, GameEngine.Unknown)
            };

            levelMetadata.BaseUrl = BaseUrl;
            levelMetadata.Title = levelTitle;
            levelMetadata.SourceSiteDisplayName = DisplayName;

            var levelDescription = GetLevelDescription(level);

            levelMetadata.Description = levelDescription;

            var levelSize = GetLevelSize(level);
            levelMetadata.SizeInMb = levelSize;

            var downloadLink = level.SelectSingleNodeFromElement(".//a[contains(@class,'docman_download__button')]");

            if (downloadLink == null)
            {
                Logger.LogInformation("Download button not found! Skip");
                continue;
            }

            var detailsLink = level.SelectSingleNodeFromElement(".//a[contains(@class, 'koowa_header__title_link')]");
            if (detailsLink != null)
            {
                var detailsUrl = new Uri(new Uri(BaseUrl), detailsLink.GetAttributeValue("href")).ToString();
                levelMetadata.DetailsLink = detailsUrl;
            }

            var downloadUrl = new Uri(new Uri(BaseUrl), downloadLink.GetAttributeValue("href")).ToString();
            levelMetadata.DownloadLink = downloadUrl;

            var screenshot = level.QuerySelectorAll(".ig-slideshow-item:first-child .ig-slideshow-image").FirstOrDefault();

            var attr = screenshot?.Attributes["data-ig-lazy-src"];
            if (attr != null)
            {
                levelMetadata.TitlePic = new Uri(new Uri(BaseUrl), attr.Value).ToString();
            }
            
            allLevels.Add(levelMetadata);
        }

        var filteredLevels = ApplyClientSideFilters(allLevels, payload);

        return new SearchResultPage(filteredLevels, 1);
    }

    private int? GetLevelSize(IElement level)
    {
        var dlButton = level.SelectSingleNodeFromElement(".//span[contains(@class,'docman_download__info')]");
        if (dlButton == null) return null;

        var text = dlButton.GetInnerHtml();
        if (text.IsNullOrWhiteSpace()) return null;

        var match = SizeRegex().Match(text);
        if (!match.Success) return null;

        return double.TryParse(match.Groups["SIZE"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var mb)
            ? (int)Math.Ceiling(mb)
            : null;
    }

    private static string? GetLevelDescription(IElement level)
    {
        var descElement = level.SelectSingleNodeFromElement(".//div[@itemprop='description']") as IElement;
        if (descElement == null) return null;

        return string.Concat(
            descElement.ChildNodes
                .TakeWhile(n => n is not IElement el || !el.ClassList.Contains("ig-main-scope-wrapper"))
                .Select(n => n.TextContent)
        ).Trim().Replace("\n", "\n\n");
    }

    private List<IGameSearchResultMetadata> ApplyClientSideFilters(List<IGameSearchResultMetadata> source, DownloaderSearchPayload payload)
    {
        IEnumerable<IGameSearchResultMetadata> output = source;

        if (payload.LevelName.IsNotNullOrWhiteSpace())
        {
            output = source.Where(l =>
                l.Title.Contains(payload.LevelName!, StringComparison.InvariantCultureIgnoreCase));
        }
        return output.ToList();
    }

    private string? GetLevelTitle(INode level)
    {
        var levelNameNode = level.SelectSingleNodeFromElement(".//span[@itemprop='name']");
        return levelNameNode.GetInnerHtml();
    }

    public override async Task<IGameMetadata> FetchDetails(IGameSearchResultMetadata game, CancellationToken cancellationToken)
    {
        byte[] titlePic = [];
        if (game.TitlePic.IsNotNullOrWhiteSpace())
        {
            var ms = new MemoryStream();
            await HttpClient.DownloadAsync(game.TitlePic!, ms, cancellationToken: cancellationToken);
            titlePic = ms.ToArray();
        }

        return new GameMetadataDto()
        {
            Description = game.Description ?? "",
            Title = game.Title,
            TitlePicUrl = game.TitlePic,
            TitlePic = titlePic,
            Author = game.Author,
            GameEngine = game.Engine
        };
    }

    public override async Task<IGameSearchResultMetadata?> FetchDetails(string detailsUrl, CancellationToken cancellationToken)
    {
        var data = await FetchPage(new DownloaderSearchPayload(), 1, cancellationToken);

        var levelIdentifier = detailsUrl.Split('/').LastOrDefault();
        if (levelIdentifier.IsNullOrWhiteSpace())
            return null;

        var matchingLink = data.Results.FirstOrDefault(r => r.DetailsLink?.Split('/').LastOrDefault() == levelIdentifier);
        if (matchingLink == null)
            return null;
        
        var details = await FetchDetails(matchingLink, cancellationToken);

        return new GameSearchResultMetadataDto()
        {
            BaseUrl = BaseUrl,
            SourceSiteDisplayName = DisplayName,
            Author = details.Author,
            Engine = details.GameEngine,
            Description = details.Description,
            Length = details.Length,
            TitlePic = details.TitlePicUrl,
            Title = details.Title,
            DetailsLink = matchingLink.DetailsLink,
            DownloadLink = matchingLink.DownloadLink,
            Rating = matchingLink.Rating,
            ReviewsLink = matchingLink.ReviewsLink,
            SizeInMb = matchingLink.SizeInMb,
            WalkthroughLink = matchingLink.WalkthroughLink,
            AuthorFullName = matchingLink.AuthorFullName,
            Difficulty = matchingLink.Difficulty,
            ReleaseDate = matchingLink.ReleaseDate,
            ReviewCount = matchingLink.ReviewCount,
            Setting = matchingLink.Setting
        };
    }

    public override Task DownloadGame(IGameSearchResultMetadata metadata, Stream stream, IProgress<DownloadProgressInfo> downloadProgress,
        CancellationToken cancellationToken)
    {
        return HttpClient.DownloadAsync(metadata.DownloadLink!, stream, downloadProgress, cancellationToken);
    }

    public RaidingTheGlobeGameDownloader(IHttpClientFactory httpClientFactory, ILogger<RaidingTheGlobeGameDownloader> logger) : base(httpClientFactory, logger)
    {
    }

    [GeneratedRegex(@"\(.+?,\s*(?<SIZE>\d+\.?\d*)\s*(?<UNIT>MB|KB)\)")]
    private static partial Regex SizeRegex();
}