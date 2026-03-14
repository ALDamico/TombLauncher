using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Extensions;
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


    private async Task ParsePage(HtmlDocument htmlDocument, List<IGameSearchResultMetadata> result)
    {
        var levelsList = htmlDocument.DocumentNode.SelectNodes("//div[@class='levels']/article");
        if (levelsList.IsNullOrEmpty())
            return;
        foreach (var level in levelsList)
        {
            var searchResult = new GameSearchResultMetadataDto
            {
                BaseUrl = BaseUrl,
                SourceSiteDisplayName = DisplayName
            };
            var headerNode = level.SelectSingleNode("./div[@class='level-content-block']/header");
            var author = headerNode.SelectSingleNode("./h2/em/a")?.InnerText?.Trim();
            searchResult.Author = author;

            var titleNode = headerNode.SelectSingleNode("./h2/a");
            if (titleNode != null)
            {
                var targetEncoding = TryDetectEncoding(titleNode);

                searchResult.Title = Encoding.UTF8.GetString(targetEncoding.GetBytes(titleNode.InnerText));
                searchResult.DetailsLink = titleNode.Attributes["href"].Value;
            }

            var releaseDateNode = headerNode.SelectSingleNode("./time");
            if (releaseDateNode != null)
            {
                if (DateTime.TryParse(releaseDateNode.Attributes["datetime"].Value, out var releaseDate))
                {
                    searchResult.ReleaseDate = releaseDate;
                }
            }

            var ratings = headerNode.SelectNodes("./div[@class='stars']/a/i");
            if (ratings != null)
            {
                searchResult.Rating = ratings.Count(n => n.HasClass("fa-star")) * 2 +
                                      ratings.Count(n => n.HasClass("fa-star-half-o"));
            }

            var featuredImage = level.SelectSingleNode("./div[@class='level-featured-image']/a/img");
            if (featuredImage != null)
            {
                searchResult.TitlePic = featuredImage.Attributes["src"].Value;
            }

            var engineTypeNode = level.SelectSingleNode("./div[contains(@class,'level-content')]");
            if (engineTypeNode != null)
            {
                var innerText = engineTypeNode.InnerText.Remove(" ");
                var regex = new Regex(@"((TombRaider\d|TEN))");
                var matches = regex.Match(innerText);
                if (matches.Success)
                {
                    Enum.TryParse<GameEngine>(matches.Groups[1].Value, true, out var engine);
                    searchResult.Engine = engine;
                }
            }

            var buttonsLinks = level.SelectSingleNode("./div[contains(@class, 'infos-buttons')]").SelectNodes("./a");
            foreach (var link in buttonsLinks)
            {
                var linkText = link.Attributes["href"].Value;
                if (link.HasClass("info") && searchResult.DetailsLink.IsNullOrWhiteSpace())
                {
                    searchResult.DetailsLink = linkText;
                }
                else if (link.HasClass("reviews") && linkText != "#")
                {
                    searchResult.ReviewsLink = linkText;
                }
                else if (link.HasClass("walkthroughs") && linkText != "#")
                {
                    searchResult.WalkthroughLink = linkText;
                }
                else if (link.HasClass("download"))
                {
                    searchResult.DownloadLink = linkText;
                }
            }

            result.Add(searchResult);
        }

        await Task.CompletedTask;
    }

    private static Encoding TryDetectEncoding(HtmlNode titleNode)
    {
        var detection = CharsetDetector.DetectFromBytes(Encoding.GetEncoding(1252).GetBytes(titleNode.InnerText));
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


    public override async Task<List<IGameSearchResultMetadata>> FetchPage(int pageNumber, CancellationToken cancellationToken)
    {
        var result = new List<IGameSearchResultMetadata>();
        if (pageNumber > TotalPages) return result;
        var convertedRequest = ConvertRequest(DownloaderSearchPayload);
        var kvpList = ConvertRequest(convertedRequest);

        var urlEncodedContent = new FormUrlEncodedContent(kvpList);
        var queryString = await urlEncodedContent.ReadAsStringAsync(cancellationToken);
        var url = GetPageUrl(pageNumber, queryString);
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, url) { Content = urlEncodedContent };
        var response = await HttpClient.SendAsync(requestMessage, cancellationToken);
        var content = await response.Content.ReadAsStreamAsync(cancellationToken);

        var htmlDocument = new HtmlDocument();
        htmlDocument.Load(content);
        if (TotalPages == null)
        {
            TotalPages = GetTotalPages(htmlDocument);
        }

        await ParsePage(htmlDocument, result);
        return result;
    }

    private string GetPageUrl(int pageNumber, string queryString)
    {
        var baseUrl = "/trle/levels/";
        if (pageNumber > 1)
        {
            baseUrl += $"page/{pageNumber}/";
        }

        return baseUrl + "?" + queryString;
    }

    public override Task DownloadGame(IGameSearchResultMetadata metadata, Stream stream,
        IProgress<DownloadProgressInfo> downloadProgress, CancellationToken cancellationToken)
    {
        return HttpClient.DownloadAsync(metadata.DownloadLink, stream, downloadProgress, cancellationToken);
    }

    public override async Task<IGameMetadata> FetchDetails(IGameSearchResultMetadata game,
        CancellationToken cancellationToken)
    {
        var detailsLink = game.DetailsLink;
        var page = await HttpClient.GetStreamAsync(detailsLink, cancellationToken);
        var htmlDocument = new HtmlDocument();
        htmlDocument.Load(page);
        var dto = new GameMetadataDto()
        {
            Author = game.Author,
            Difficulty = game.Difficulty,
            Length = game.Length,
            Setting = game.Setting,
            Title = game.Title,
            ReleaseDate = game.ReleaseDate,
            TitlePic = await HttpClient.GetByteArrayAsync(game.TitlePic, cancellationToken),
            GameEngine = game.Engine,
            AuthorFullName = game.AuthorFullName
        };

        var detailsDiv = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'level-content')]");
        if (detailsDiv != null)
        {
            var detailsText = detailsDiv.InnerHtml.Trim();
            if (dto.Description == null || detailsText?.Length > dto.Description?.Length)
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

    private AspideTrSearchRequest ConvertRequest(DownloaderSearchPayload request)
    {
        return new AspideTrSearchRequest()
        {
            LAuthor = request.AuthorName ?? string.Empty,
            LTitle = request.LevelName ?? string.Empty,
            LType = ConvertEngine(request.GameEngine.GetValueOrDefault()) ?? 0,
            Classes = string.Join("-", ConvertFlags(request))
        };
    }

    private IEnumerable<KeyValuePair<string, string>> ConvertRequest(AspideTrSearchRequest convertedSearchRequest)
    {
        return convertedSearchRequest.ToQueryParams();
    }

    private int GetTotalPages(HtmlDocument htmlDocument)
    {
        var paginationDiv = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='pagination']");

        var lastListItem = paginationDiv?.SelectNodes("./ul/li").LastOrDefault();
        if (lastListItem == null) 
            return 1;
        return int.TryParse(lastListItem.SelectSingleNode("./a").InnerText, out var val) ? val : 1;
    }
}