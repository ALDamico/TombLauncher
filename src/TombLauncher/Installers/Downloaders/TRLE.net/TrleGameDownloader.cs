using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
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

namespace TombLauncher.Installers.Downloaders.TRLE.net;

public class TrleGameDownloader : GameDownloaderBase
{
    public TrleGameDownloader(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
    {
        _gameEngineMapping = new Dictionary<GameEngine, string>()
        {
            { GameEngine.Unknown, string.Empty },
            { GameEngine.TombRaider1, "TR1" },
            { GameEngine.TombRaider2, "TR2" },
            { GameEngine.TombRaider3, "TR3" },
            { GameEngine.TombRaider4, "TR4" },
            { GameEngine.TombRaider5, "TR5" },
            { GameEngine.Ten, "TEN" }
        };
        _inverseGameEngineMapping = _gameEngineMapping.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }

    private const int RowsPerPage = 20;


    public override string DisplayName => "TRLE.net";
    public override string BaseUrl => "https://trle.net";
    private readonly Dictionary<GameEngine, string> _gameEngineMapping;
    private readonly Dictionary<string, GameEngine> _inverseGameEngineMapping;

    private int GetTotalRows(HtmlDocument htmlDocument)
    {
        var nodeWithTotal = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='navText']");
        if (nodeWithTotal == null)
        {
            return -1;
        }

        var nodeText = nodeWithTotal.InnerText;
        var regex = new Regex(@"(\d+) record\(s\) found");
        var match = regex.Match(nodeText);

        if (int.TryParse(match.Groups[1].Value, out var result))
        {
            return result;
        }

        return -1;
    }

    public override async Task<List<IGameSearchResultMetadata>> GetGames(DownloaderSearchPayload searchPayload,
        CancellationToken cancellationToken = default)
    {
        DownloaderSearchPayload = searchPayload;
        cancellationToken.ThrowIfCancellationRequested();
        CurrentPage = 0;
        TotalPages = null;

        var result = await FetchNextPage(cancellationToken);

        return result;
    }

    public override async Task<List<IGameSearchResultMetadata>> FetchNextPage(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (CurrentPage >= TotalPages) return new List<IGameSearchResultMetadata>();
        CurrentPage++;
        try
        {
            return await FetchPage(CurrentPage, cancellationToken);
        }
        catch (HttpRequestException)
        {
            CurrentPage--;
            return new List<IGameSearchResultMetadata>();
        }
    }

    public override async Task<List<IGameSearchResultMetadata>> FetchPage(int pageNumber, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (pageNumber > TotalPages) return new List<IGameSearchResultMetadata>();
        var result = new List<IGameSearchResultMetadata>();
        var request = ConvertRequest(DownloaderSearchPayload, pageNumber);
        var requestStrng = request.ToQueryParams();
        var urlEncodedContent = new FormUrlEncodedContent(requestStrng);
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/pFind.php");
        requestMessage.Content = urlEncodedContent;

        var response = await HttpClient.SendAsync(requestMessage, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(content);
        if (TotalPages == null)
            TotalPages = (int)Math.Ceiling((double)GetTotalRows(htmlDocument) / RowsPerPage);

        ParseResultPage(htmlDocument, result, cancellationToken);
        return result;
    }

    private void ParseResultPage(HtmlDocument htmlDocument, List<IGameSearchResultMetadata> result,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var resultsTable = htmlDocument.DocumentNode.SelectSingleNode("//table[@class='FindTable']");
        var rows = resultsTable?.SelectNodes("./tr");
        var headerRow = rows?.FirstOrDefault()?.SelectNodes("./strong/td");
        if (rows == null || headerRow == null) return;
        var dataRows = rows.Skip(1);
        foreach (var row in dataRows)
        {
            var metadata = new GameSearchResultMetadataDto() { BaseUrl = BaseUrl, SourceSiteDisplayName = DisplayName };
            var fields = row.SelectNodes("./td");
            var zipped = headerRow.Zip(fields,
                (header, r) => new KeyValuePair<string, HtmlNode>(header.InnerText.Trim(),
                    r));
            foreach (var zip in zipped)
            {
                var value = zip.Value;
                if (value == null) continue;
                var v = value.InnerText?.Trim().IsNullOrEmpty() == true
                    ? value.SelectSingleNode("./a")?.Attributes["href"]?.Value
                    : value.InnerText?.Trim() ?? string.Empty;
                if (v == null) continue;
                switch (zip.Key)
                {
                    case "nickname":
                        metadata.Author = v;
                        break;
                    case "author's name":
                        metadata.AuthorFullName = v;
                        break;
                    case "levelname/info":
                        metadata.Title = v;
                        var detailsLink = value.SelectSingleNode("./a")?.Attributes["href"]?.Value;
                        metadata.DetailsLink = detailsLink ?? string.Empty;
                        break;
                    case "difficulty":
                        metadata.Difficulty = Enum.TryParse<GameDifficulty>(v, true, out var actualDifficulty)
                            ? actualDifficulty
                            : GameDifficulty.Unknown;
                        break;
                    case "duration":
                        metadata.Length = Enum.TryParse<GameLength>(v, true, out var actualLength)
                            ? actualLength
                            : GameLength.Unknown;
                        break;
                    case "class":
                        metadata.Setting = v;
                        break;
                    case "size (MB)":
                        if (int.TryParse(v, out var size))
                            metadata.SizeInMb = size;
                        break;
                    case "type":
                        metadata.Engine = _inverseGameEngineMapping[v];
                        break;
                    case "rating":
                        if (double.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var rating))
                            metadata.Rating = rating;
                        break;
                    case "reviews":
                        if (int.TryParse(v, out var reviewCount))
                            metadata.ReviewCount = reviewCount;
                        break;
                    case "released":
                        if (DateTime.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.None, out var releasedDate))
                            metadata.ReleaseDate = releasedDate;
                        break;
                }


                if (v.Contains("reviews.php"))
                {
                    var actualValue = v;
                    if (actualValue.StartsWith("/"))
                    {
                        actualValue = BaseUrl + actualValue;
                    }

                    metadata.ReviewsLink = actualValue;
                }

                if (v.Contains("trle_dl.php"))
                {
                    var actualValue = v;
                    if (actualValue.StartsWith("/"))
                    {
                        actualValue = BaseUrl + actualValue;
                    }

                    metadata.DownloadLink = actualValue;
                }

                if (v.Contains("Levelwalk.php"))
                {
                    var actualValue = v;
                    if (actualValue.StartsWith("/"))
                    {
                        actualValue = BaseUrl + actualValue;
                    }

                    metadata.WalkthroughLink = actualValue;
                }
            }

            result.Add(metadata);
        }
    }

    public override Task DownloadGame(IGameSearchResultMetadata metadata, Stream stream,
        IProgress<DownloadProgressInfo> downloadProgress, CancellationToken cancellationToken)
    {
        return HttpClient.DownloadAsync(metadata.DownloadLink, stream, downloadProgress, cancellationToken);
    }

    public override async Task<IGameMetadata> FetchDetails(IGameSearchResultMetadata game,
        CancellationToken cancellationToken)
    {
        var detailsUrl = game.DetailsLink;
        var page = await HttpClient.GetStreamAsync(detailsUrl, cancellationToken);
        var htmlDocument = new HtmlDocument();
        htmlDocument.Load(page);
        var metadata = new GameMetadataDto
        {
            Author = game.Author,
            Difficulty = game.Difficulty,
            Length = game.Length,
            Setting = game.Setting,
            Title = game.Title,
            GameEngine = game.Engine,
            ReleaseDate = game.ReleaseDate,
            AuthorFullName = game.AuthorFullName
        };

        var imageNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@align='center']/img[@class='border']");
        if (imageNode != null)
        {
            var uri = imageNode.Attributes["src"].Value;
            var byteArr = await HttpClient.GetByteArrayAsync(uri, cancellationToken);
            if (game.TitlePic.IsNullOrWhiteSpace())
            {
                game.TitlePic = new Uri(new Uri(game.BaseUrl), uri).ToString();
            }
            metadata.TitlePic = byteArr;
        }

        var descriptionNode =
            htmlDocument.DocumentNode.SelectNodes("//td[@class='medGText']").LastOrDefault();
        if (descriptionNode != null)
        {
            metadata.Description = descriptionNode.InnerText;
        }

        return metadata;
    }

    public override DownloaderFeatures SupportedFeatures => DownloaderFeatures.Author | DownloaderFeatures.LevelName |
                                                   DownloaderFeatures.GameEngine | DownloaderFeatures.GameDifficulty |
                                                   DownloaderFeatures.Rating | DownloaderFeatures.GameLength;

    private TrleSearchRequest ConvertRequest(DownloaderSearchPayload searchPayload, int pageNumber)
    {
        int? difficulty = null;
        if (searchPayload.GameDifficulty != null && searchPayload.GameDifficulty != GameDifficulty.Unknown)
        {
            difficulty = (int)searchPayload.GameDifficulty;
        }

        var gameEngine = _gameEngineMapping[searchPayload.GameEngine.GetValueOrDefault()];
        int? duration = null;
        if (searchPayload.Duration != null && searchPayload.Duration.GetValueOrDefault() != GameLength.Unknown)
        {
            duration = (int)(searchPayload.Duration.Value);
        }

        return new TrleSearchRequest()
        {
            Atype = 1,
            Author = searchPayload.AuthorName ?? string.Empty,
            Class = string.Empty,
            Difficulty = difficulty, // TODO verify these value match
            Level = searchPayload.LevelName ?? string.Empty,
            Rating = searchPayload.Rating,
            Sorttype = 2,
            Type = gameEngine,
            DurationClass = duration,
            SortIdx = 8,
            Idx = (pageNumber - 1) * RowsPerPage
        };
    }
}