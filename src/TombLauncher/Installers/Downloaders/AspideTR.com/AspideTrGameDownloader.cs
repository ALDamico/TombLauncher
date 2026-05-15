using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.XPath;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Extensions;
using TombLauncher.Utils;
using UtfUnknown;

namespace TombLauncher.Installers.Downloaders.AspideTR.com;

public class AspideTrGameDownloader : GameDownloaderBase
{
    public AspideTrGameDownloader(IHttpClientFactory httpClientFactory, Dictionary<string, string> classMappings) : base(httpClientFactory)
    {
        _classMappings = classMappings;
    }

    public override string DisplayName => "AspideTR";
    public override string BaseUrl => "https://www.aspidetr.com/";


    private readonly Dictionary<GameEngine, int?> _gameEngineMappings = new()
    {
        { GameEngine.Unknown, null },
        { GameEngine.Ten, 8 },
        { GameEngine.TombRaider1, 5 },
        { GameEngine.TombRaider2, 4 },
        { GameEngine.TombRaider3, 3 },
        { GameEngine.TombRaider4, 2 },
        { GameEngine.TombRaider5, 7 }
    };

    private readonly Dictionary<string, string> _classMappings;


    private async Task ParsePage(IDocument htmlDocument, List<IGameSearchResultMetadata> result)
    {
        var levelsList = htmlDocument.Body.SelectNodes("//div[@class='levels']/article");
        if (levelsList.IsNullOrEmpty())
            return;
        foreach (var level in levelsList)
        {
            var searchResult = new GameSearchResultMetadataDto
            {
                BaseUrl = BaseUrl,
                SourceSiteDisplayName = DisplayName
            };
            var headerNode = level?.SelectSingleNodeFromElement("./div[@class='level-content-block']/header");
            var author = headerNode.SelectSingleNodeFromElement("./h2/em/a")?.TextContent.Trim();
            searchResult.Author = author;

            var titleNode = headerNode.SelectSingleNodeFromElement("./h2/a");
            if (titleNode != null)
            {
                var targetEncoding = TryDetectEncoding(titleNode);

                searchResult.Title = Encoding.UTF8.GetString(targetEncoding.GetBytes(titleNode.TextContent));
                searchResult.DetailsLink = titleNode.GetAttributeValue("href");
            }

            var releaseDateNode = headerNode.SelectSingleNodeFromElement("./time");
            if (releaseDateNode != null)
            {
                if (DateTime.TryParse(releaseDateNode.GetAttributeValue("datetime"), out var releaseDate))
                {
                    searchResult.ReleaseDate = releaseDate;
                }
            }

            var ratings = headerNode.SelectNodesFromElement("./div[@class='stars']/a/i");
            if (ratings.IsNotNullOrEmpty())
            {
                searchResult.Rating = ratings.Count(n => n.HasClass("fa-star")) * 2 +
                                      ratings.Count(n => n.HasClass("fa-star-half-o"));
            }

            var featuredImage = level.SelectSingleNodeFromElement("./div[@class='level-featured-image']/a/img");
            if (featuredImage != null)
            {
                searchResult.TitlePic = featuredImage.GetAttributeValue("src");
            }

            var engineTypeNode = level.SelectSingleNodeFromElement("./div[contains(@class,'level-content')]");
            if (engineTypeNode != null)
            {
                var innerText = engineTypeNode.TextContent.Remove(" ");
                var regex = new Regex(@"((TombRaider\d|TEN))");
                var matches = regex.Match(innerText);
                if (matches.Success)
                {
                    Enum.TryParse<GameEngine>(matches.Groups[1].Value, true, out var engine);
                    searchResult.Engine = engine;
                }
            }

            var buttonsLinks = level.SelectSingleNodeFromElement("./div[contains(@class, 'infos-buttons')]").SelectNodesFromElement("./a");
            foreach (var link in buttonsLinks)
            {
                var linkText = link.GetAttributeValue("href");
                if (link.HasClass("info") && searchResult.DetailsLink.IsNullOrWhiteSpace())
                {
                    searchResult.DetailsLink = linkText.EnsureStartsWith(BaseUrl, '/');
                }
                else if (link.HasClass("reviews") && linkText != "#")
                {
                    searchResult.ReviewsLink = linkText.EnsureStartsWith(BaseUrl, '/');
                }
                else if (link.HasClass("walkthroughs") && linkText != "#")
                {
                    searchResult.WalkthroughLink = linkText.EnsureStartsWith(BaseUrl, '/');
                }
                else if (link.HasClass("download"))
                {
                    searchResult.DownloadLink = linkText.EnsureStartsWith(BaseUrl, '/');
                }
            }

            result.Add(searchResult);
        }

        await Task.CompletedTask;
    }

    private static Encoding TryDetectEncoding(INode titleNode)
    {
        var detection = CharsetDetector.DetectFromBytes(Encoding.GetEncoding(1252).GetBytes(titleNode.TextContent));
        if (detection.Details.IsNullOrEmpty())
            return Encoding.UTF8;
        var details = detection.Details.ToList();
        var targetEncoding = details[0].Encoding;
        if (details.Count <= 1) 
            return targetEncoding;
        if (!Equals(details[0].Encoding, Encoding.UTF8)) 
            return targetEncoding;
        if (Math.Abs(details[0].Confidence - details[1].Confidence) <= 0.05)
        {
            targetEncoding = details[1].Encoding;
        }

        return targetEncoding;
    }


    protected override async Task<ISearchResultPage> FetchPage(DownloaderSearchPayload payload, int pageNumber, CancellationToken cancellationToken)
    {
        var result = new List<IGameSearchResultMetadata>();
        var kvpList = ConvertRequest(payload);

        var urlEncodedContent = new FormUrlEncodedContent(kvpList);
        var queryString = await urlEncodedContent.ReadAsStringAsync(cancellationToken);
        var url = GetPageUrl(pageNumber, queryString);

        var htmlDocument = await AppUtils.OpenDocument(url, cancellationToken);
        var totalPages = GetTotalPages(htmlDocument);

        await ParsePage(htmlDocument, result);
        return new SearchResultPage(result, totalPages);
    }

    private string GetPageUrl(int pageNumber, string queryString)
    {
        var baseUrl = "/trle/levels/";
        if (pageNumber > 1)
        {
            baseUrl += $"page/{pageNumber}/";
        }

        var websiteUrl = BaseUrl.TrimEnd("/").ToString();

        return websiteUrl + baseUrl + "?" + queryString;
    }

    public override Task DownloadGame(IGameSearchResultMetadata metadata, Stream stream,
        IProgress<DownloadProgressInfo> downloadProgress, CancellationToken cancellationToken)
    {
        return HttpClient.DownloadAsync(metadata.DownloadLink!, stream, downloadProgress, cancellationToken);
    }

    public override async Task<IGameMetadata> FetchDetails(IGameSearchResultMetadata game,
        CancellationToken cancellationToken)
    {
        var detailsLink = new Uri(new Uri(BaseUrl), game.DetailsLink).ToString();
        var htmlDocument = await AppUtils.OpenDocument(detailsLink, cancellationToken);
        var dto = new GameMetadataDto()
        {
            Author = game.Author,
            Difficulty = game.Difficulty,
            Length = game.Length,
            Setting = game.Setting,
            Title = game.Title,
            ReleaseDate = game.ReleaseDate,
            TitlePic = await HttpClient.GetByteArrayAsync(game.TitlePic, cancellationToken),
            TitlePicUrl = game.TitlePic,
            GameEngine = game.Engine,
            AuthorFullName = game.AuthorFullName
        };

        var detailsDiv = htmlDocument.Body.SelectSingleNode("//div[contains(@class, 'level-content')]");
        if (detailsDiv != null)
        {
            var detailsText = detailsDiv.GetInnerHtmlOrEmpty().Trim();
            if (dto.Description.IsNullOrWhiteSpace() || detailsText.Length > dto.Description.Length)
            {
                dto.Description = detailsText;
            }
        }

        return dto;
    }

    public override DownloaderFeatures SupportedFeatures => DownloaderFeatures.Author | DownloaderFeatures.LevelName |
                                                   DownloaderFeatures.Setting | DownloaderFeatures.GameDifficulty |
                                                   DownloaderFeatures.GameLength | DownloaderFeatures.GameEngine;

    private int? ConvertEngine(GameEngine engine)
    {
        return _gameEngineMappings[engine];
    }

    private List<int> ConvertFlags(DownloaderSearchPayload request)
    {
        var flags = new List<int>();
        if (request.GameDifficulty >= GameDifficulty.Challenging)
        {
            flags.Add(AspideTrConstants.HighDifficulty);
        }

        if (request.Setting != null)
        {
            var distinctSettings = request.Setting.Split(",")
                .Select(s => s.Trim().ToLowerInvariant().RemoveDiacritics())
                .ToArray();
            foreach (var setting in distinctSettings)
            {
                if (_classMappings.TryGetValue(setting, out var detectedFlag))
                {
                    if (AspideTrConstants.Mappings.TryGetValue(detectedFlag, out var intValue))
                    {
                        flags.Add(intValue);
                    }
                }
            }
        }


        return flags;
    }

    private IEnumerable<KeyValuePair<string, string>> ConvertRequest(DownloaderSearchPayload request)
    {
        return new AspideTrSearchRequest()
        {
            LAuthor = request.AuthorName ?? string.Empty,
            LTitle = request.LevelName ?? string.Empty,
            LType = ConvertEngine(request.GameEngine.GetValueOrDefault()) ?? 0,
            Classes = string.Join("-", ConvertFlags(request))
        }.ToQueryParams();
    }

    private int GetTotalPages(IDocument htmlDocument)
    {
        var paginationDiv = htmlDocument.Body.SelectSingleNode("//div[@class='pagination']");

        var lastListItem = paginationDiv.SelectNodesFromElement("./ul/li").LastOrDefault();
        if (lastListItem == null) 
            return 1;
        return int.TryParse(lastListItem.SelectSingleNodeFromElement("./a")?.TextContent, out var val) ? val : 1;
    }
}