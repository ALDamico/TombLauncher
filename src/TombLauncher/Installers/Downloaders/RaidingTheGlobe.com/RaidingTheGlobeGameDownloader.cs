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

public class RaidingTheGlobeGameDownloader : GameDownloaderBase
{
    private Dictionary<string, GameEngine> _enginesLookup = new()
    {
        { "Lara Croft Tomb Raider: The Scion of Qualopec", GameEngine.TombRaider4 },
        { "Lara Croft Tomb Raider: Afterlife", GameEngine.TombRaider4 }
    };
    
    public override string DisplayName => "Raiding the Globe";
    public override string BaseUrl => "https://www.raidingtheglobe.com";
    public override DownloaderFeatures SupportedFeatures => DownloaderFeatures.LevelName;
    protected override async Task<ISearchResultPage> FetchPage(DownloaderSearchPayload payload, int pageNumber, CancellationToken cancellationToken)
    {
        var uriBuilder = new UriBuilder(new Uri(BaseUrl));
        uriBuilder.Path = "/downloads/custom-games-tomb-raider-level-editor";

        var htmlDocument = await AppUtils.OpenDocument(uriBuilder.Uri.ToString(), cancellationToken);

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

        var match = Regex.Match(text!, @"\(.+?,\s*(?<SIZE>\d+\.?\d*)\s*(?<UNIT>MB|KB)\)");
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

    public override Task DownloadGame(IGameSearchResultMetadata metadata, Stream stream, IProgress<DownloadProgressInfo> downloadProgress,
        CancellationToken cancellationToken)
    {
        return HttpClient.DownloadAsync(metadata.DownloadLink!, stream, downloadProgress, cancellationToken);
    }

    public RaidingTheGlobeGameDownloader(IHttpClientFactory httpClientFactory, ILogger<RaidingTheGlobeGameDownloader> logger) : base(httpClientFactory, logger)
    {
    }
}