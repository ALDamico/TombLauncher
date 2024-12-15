using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Dtos;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Progress;
using TombLauncher.Contracts.Utils;
using TombLauncher.Core.Extensions;
using TombLauncher.Extensions;

namespace TombLauncher.Installers.Downloaders.AspideTR.com;

public class AspideTrGameDownloader : IGameDownloader
{
    public AspideTrGameDownloader()
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(BaseUrl),
        };
        _classMappings = new Dictionary<string, string>();
    }
    public AspideTrGameDownloader(Dictionary<string, string> classMappings) : this()
    {
        _classMappings = classMappings;
    }

    public string DisplayName => "AspideTR";
    public string BaseUrl => "https://www.aspidetr.com/";
    public DownloaderSearchPayload DownloaderSearchPayload { get; private set; }

    private readonly HttpClient _httpClient;

    private readonly Dictionary<GameEngine, int?> _gameEngineMappings = new Dictionary<GameEngine, int?>()
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

    public async Task<List<IGameSearchResultMetadata>> GetGames(DownloaderSearchPayload searchPayload,
        CancellationToken cancellationToken)
    {
        DownloaderSearchPayload = searchPayload;
        cancellationToken.ThrowIfCancellationRequested();
        CurrentPage = 0;

        var result = await FetchNextPage(cancellationToken);

        return result;
    }

    private async Task ParsePage(HtmlDocument htmlDocument, List<IGameSearchResultMetadata> result)
    {
        var levelsList = htmlDocument.DocumentNode.SelectNodes("//div[@class='levels']/article");
        if (levelsList == null)
            return;
        foreach (var level in levelsList)
        {
            var searchResult = new GameSearchResultMetadataDto();
            searchResult.BaseUrl = BaseUrl;
            var headerNode = level.SelectSingleNode("./div[@class='level-content-block']/header");
            var authorNode = headerNode.SelectSingleNode("./h2/em/a");
            if (authorNode != null)
            {
                searchResult.Author = authorNode.InnerText.Trim();
            }

            var titleNode = headerNode.SelectSingleNode("./h2/a");
            if (titleNode != null)
            {
                searchResult.Title = titleNode.InnerText;
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
                searchResult.TitlePic = await _httpClient.GetByteArrayAsync(featuredImage.Attributes["src"].Value);
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
                if (link.HasClass("info") && string.IsNullOrWhiteSpace(searchResult.DetailsLink))
                {
                    searchResult.DetailsLink = linkText;
                }
                else if (link.HasClass("reviews"))
                {
                    searchResult.ReviewsLink = linkText;
                }
                else if (link.HasClass("walkthroughs"))
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
    }

    public async Task<List<IGameSearchResultMetadata>> FetchNextPage(CancellationToken cancellationToken)
    {
        var result = new List<IGameSearchResultMetadata>();
        if (CurrentPage > TotalPages) return result;
        CurrentPage++;
        var convertedRequest = ConvertRequest(DownloaderSearchPayload);
        var kvpList = ConvertRequest(convertedRequest);

        var urlEncodedContent = new FormUrlEncodedContent(kvpList);
        var queryString = await urlEncodedContent.ReadAsStringAsync();
        var url = GetPageUrl(CurrentPage, queryString);
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, url) { Content = urlEncodedContent };
        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
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

    public Task DownloadGame(IGameSearchResultMetadata metadata, Stream stream,
        IProgress<DownloadProgressInfo> downloadProgress, CancellationToken cancellationToken)
    {
        return _httpClient.DownloadAsync(metadata.DownloadLink, stream, downloadProgress, cancellationToken);
    }

    public async Task<GameMetadataDto> FetchDetails(IGameSearchResultMetadata game,
        CancellationToken cancellationToken)
    {
        var detailsLink = game.DetailsLink;
        var page = await _httpClient.GetStreamAsync(detailsLink, cancellationToken);
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
            TitlePic = game.TitlePic,
            GameEngine = game.Engine,
            AuthorFullName = game.AuthorFullName
        };

        var detailsDiv = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'level-content')]");
        if (detailsDiv != null)
        {
            var detailsText = detailsDiv.InnerText;
            if (dto.Description == null || detailsText?.Length > dto.Description?.Length)
            {
                dto.Description = detailsText;
            }
        }

        return dto;
    }

    public bool HasMorePages()
    {
        return CurrentPage < TotalPages;
    }

    public int? TotalPages { get; private set; }
    public int CurrentPage { get; private set; }

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
            LAuthor = request.AuthorName,
            LTitle = request.LevelName,
            LType = ConvertEngine(request.GameEngine.GetValueOrDefault()),
            Classes = string.Join("-", ConvertFlags(request))
        };
    }

    private IEnumerable<KeyValuePair<string, string>> ConvertRequest(AspideTrSearchRequest convertedSearchRequest)
    {
        return ReflectionUtils.GetPropertiesAsKeyValuePairs(convertedSearchRequest, k => k.ToLowerInvariant());
    }

    private int GetTotalPages(HtmlDocument htmlDocument)
    {
        var paginationDiv = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='pagination']");

        var lastListItem = paginationDiv?.SelectNodes("./ul/li").LastOrDefault();
        if (lastListItem == null) return 1;
        return int.TryParse(lastListItem.SelectSingleNode("./a").InnerText, out var val) ? val : 1;
    }
}