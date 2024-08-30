using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using TombLauncher.Dto;
using TombLauncher.Extensions;
using TombLauncher.Models;
using TombLauncher.Progress;
using TombLauncher.Utils;
using TombLauncher.ViewModels;

namespace TombLauncher.Installers.Downloaders.AspideTR.com;

public class AspideTrGameDownloader : IGameDownloader
{
    public AspideTrGameDownloader(Dictionary<string, string> classMappings)
    {
        _classMappings = classMappings;
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(BaseUrl),
        };
    }
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
    public async Task<List<GameSearchResultMetadataViewModel>> GetGames(DownloaderSearchPayload searchPayload, CancellationToken cancellationToken)
    {
        DownloaderSearchPayload = searchPayload;
        cancellationToken.ThrowIfCancellationRequested();

        var result = await FetchNextPage(cancellationToken);
        
        return result;
    }

    private void ParsePage(HtmlDocument htmlDocument, List<GameSearchResultMetadataViewModel> result)
    {
        var levelsList = htmlDocument.DocumentNode.SelectNodes("//div[@class='levels']/article");
        foreach (var level in levelsList)
        {
            var searchResult = new GameSearchResultMetadataViewModel();
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

            result.Add(searchResult);
        }
    }

    public async Task<List<GameSearchResultMetadataViewModel>> FetchNextPage(CancellationToken cancellationToken)
    {
        var result = new List<GameSearchResultMetadataViewModel>();
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
        if (TotalPages == 0)
        {
            TotalPages = GetTotalPages(htmlDocument);
        }
        ParsePage(htmlDocument, result);
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

    public Task<GameMetadataDto> DownloadGame(GameSearchResultMetadataViewModel metadata, IProgress<DownloadProgressInfo> downloadProgress)
    {
        throw new NotImplementedException();
    }

    public Task<GameMetadataDto> FetchDetails(GameSearchResultMetadataViewModel game, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public bool HasMorePages()
    {
        return CurrentPage < TotalPages;
    }

    public int TotalPages { get; private set; }
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
            var distinctSettings = request.Setting.Split(",").Select(s => s.Trim().ToLowerInvariant().RemoveDiacritics())
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
        if (paginationDiv == null) return 1;

        var lastListItem = paginationDiv.SelectNodes("./ul/li").LastOrDefault();
        if (lastListItem == null) return 1;
        var parsed = int.TryParse(lastListItem.SelectSingleNode("./a").InnerText, out var val);
        return val;
    }
}