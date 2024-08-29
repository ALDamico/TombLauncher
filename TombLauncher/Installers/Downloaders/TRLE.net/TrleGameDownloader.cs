using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using TombLauncher.Dto;
using TombLauncher.Extensions;
using TombLauncher.Models;
using TombLauncher.Progress;
using TombLauncher.Utils;
using TombLauncher.ViewModels;

namespace TombLauncher.Installers.Downloaders.TRLE.net;

public class TrleGameDownloader : IGameDownloader
{
    public TrleGameDownloader(TombRaiderLevelInstaller installer, TombRaiderEngineDetector detector, CancellationTokenSource cancellationTokenSource)
    {
        _installer = installer;
        _engineDetector = detector;
        _cancellationTokenSource = cancellationTokenSource;
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(BaseUrl),
            DefaultRequestHeaders =
            {
                Accept =
                {
                    new MediaTypeWithQualityHeaderValue("text/html"),
                    new MediaTypeWithQualityHeaderValue("application/xhtml+xml"),
                    new MediaTypeWithQualityHeaderValue("application/xml"),
                    new MediaTypeWithQualityHeaderValue("image/avif"),
                    new MediaTypeWithQualityHeaderValue("image/webp"),
                    new MediaTypeWithQualityHeaderValue("image/apng"),
                    new MediaTypeWithQualityHeaderValue("application/signed-exchange"),
                },
                Host = "trle.net",
                Referrer = new Uri("https://trle.net/pFind.php")
            }
        };
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

    private TombRaiderLevelInstaller _installer;
    private TombRaiderEngineDetector _engineDetector;
    private CancellationTokenSource _cancellationTokenSource;
    private const int RowsPerPage = 20;
    

    public string BaseUrl => "https://trle.net";
    private HttpClient _httpClient;

    private readonly string[] _defaultHeaderOrder = new string[]
    {
        "nickname",
        "author's name",
        "",
        "",
        "",
        "levelname/info",
        "difficulty",
        "duration",
        "class",
        "size (MB)",
        "type",
        "rating",
        "reviews",
        "released"
    };

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

    public async Task<List<GameSearchResultMetadataViewModel>> GetGames(DownloaderSearchPayload searchPayload, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = new List<GameSearchResultMetadataViewModel>();
        var request = ConvertRequest(searchPayload);
        var totalCount = int.MaxValue;
        do
        {
            var requestStrng = ConvertRequest(request);
            var urlEncodedContent = new FormUrlEncodedContent(requestStrng);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/pFind.php");
            requestMessage.Content = urlEncodedContent;

            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);
            if (totalCount == int.MaxValue)
                totalCount = GetTotalRows(htmlDocument);
            ParseResultPage(htmlDocument, result, cancellationToken);
            request.Idx += RowsPerPage;
            Console.WriteLine($"Parsing page {(request.Idx / RowsPerPage)}");
        } while (request.Idx < totalCount);

        return result;
    }

    private void ParseResultPage(HtmlDocument htmlDocument, List<GameSearchResultMetadataViewModel> result, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var resultsTable = htmlDocument.DocumentNode.SelectSingleNode("//table[@class='FindTable']");
        var rows = resultsTable.SelectNodes("./tr");
        var headerRow = rows.FirstOrDefault().SelectNodes("./strong/td");
        var dataRows = rows.Skip(1);
        foreach (var row in dataRows)
        {
            var metadata = new GameSearchResultMetadataViewModel();
            var fields = row.SelectNodes("./td");
            var zipped = headerRow.Zip(fields,
                (header, r) => new KeyValuePair<string, string>(header.InnerText.Trim(),
                    string.IsNullOrEmpty(r.InnerText?.Trim())
                        ? r.SelectSingleNode("./a")?.Attributes["href"].Value
                        : r.InnerText.Trim()));
            foreach (var zip in zipped)
            {
                var value = zip.Value;
                if (value == null) continue;
                switch (zip.Key)
                {
                    case "nickname":
                        metadata.Author = zip.Value;
                        break;
                    case "author's name":
                        metadata.AuthorFullName = zip.Value;
                        break;
                    case "levelname/info":
                        metadata.Title = zip.Value;
                        break;
                    case "difficulty":
                        metadata.Difficulty = Enum.TryParse<GameDifficulty>(zip.Value, out var actualDifficulty)
                            ? actualDifficulty
                            : GameDifficulty.Unknown;
                        break;
                    case "duration":
                        metadata.Length = Enum.TryParse<GameLength>(zip.Value, out var actualLength)
                            ? actualLength
                            : GameLength.Unknown;
                        break;
                    case "class":
                        metadata.Setting = zip.Value;
                        break;
                    case "size (MB)":
                        if (int.TryParse(zip.Value, out var size))
                            metadata.SizeInMb = size;
                        break;
                    case "type":
                        metadata.Engine = _inverseGameEngineMapping[zip.Value];
                        break;
                    case "rating":
                        if (double.TryParse(zip.Value, CultureInfo.InvariantCulture, out var rating))
                            metadata.Rating = rating;
                        break;
                    case "reviews":
                        if (int.TryParse(zip.Value, out var reviewCount))
                            metadata.ReviewCount = reviewCount;
                        break;
                    case "released":
                        if (DateTime.TryParse(zip.Value, CultureInfo.InvariantCulture, out var releasedDate))
                            metadata.ReleaseDate = releasedDate;
                        break;
                }


                if (zip.Value.Contains("reviews.php"))
                {
                    var actualValue = zip.Value;
                    if (actualValue.StartsWith("/"))
                    {
                        actualValue = BaseUrl + actualValue;
                    }

                    metadata.ReviewsLink = actualValue;
                }

                if (zip.Value.Contains("trle_dl.php"))
                {
                    var actualValue = zip.Value;
                    if (actualValue.StartsWith("/"))
                    {
                        actualValue = BaseUrl + actualValue;
                    }

                    metadata.DownloadLink = actualValue;
                }

                if (zip.Value.Contains("Levelwalk.php"))
                {
                    var actualValue = zip.Value;
                    if (actualValue.StartsWith("/"))
                    {
                        actualValue = BaseUrl + actualValue;
                    }

                    metadata.WalkthroughLink = actualValue;
                }
            }

            result.Add(metadata);

            //Console.WriteLine(row.InnerHtml);
        }
    }

    public async Task<GameMetadataDto> DownloadGame(GameSearchResultMetadataViewModel metadata,
        IProgress<DownloadProgressInfo> downloadProgress)
    {
        var downloadPath = PathUtils.GetRandomTempDirectory();
        var tempZipName = Path.GetRandomFileName();
        var fullFilePath = Path.Combine(downloadPath, tempZipName);
        await using var file = new FileStream(fullFilePath, FileMode.Create);
        await _httpClient.DownloadAsync(metadata.DownloadLink, file, downloadProgress);

        var dto = new GameMetadataDto()
        {
            Author = metadata.Author,
            AuthorFullName = metadata.AuthorFullName,
            Difficulty = metadata.Difficulty,
            Guid = Guid.NewGuid(),
            GameEngine = metadata.Engine,
            InstallDate = DateTime.Now,
            ReleaseDate = metadata.ReleaseDate,
            Length = metadata.Length,
            Title = metadata.Title,
            Setting = metadata.Setting
        };
        var installLocation = _installer.Install(fullFilePath, dto);
        var exePath = _engineDetector.GetGameExecutablePath(fullFilePath);
        dto.ExecutablePath = exePath;
        dto.InstallDirectory = installLocation;
        return dto;
    }

    private TrleSearchRequest ConvertRequest(DownloaderSearchPayload searchPayload)
    {
        int? difficulty = null;
        if (searchPayload.GameDifficulty != null && searchPayload.GameDifficulty != GameDifficulty.Unknown)
        {
            difficulty = (int)searchPayload.GameDifficulty;
        }

        var gameEngine = _gameEngineMapping[searchPayload.GameEngine.GetValueOrDefault()];
        int? duration = null;
        if (searchPayload.Duration.GetValueOrDefault() != GameLength.Unknown)
        {
            duration = (int)(searchPayload.Duration);
        }

        return new TrleSearchRequest()
        {
            Atype = 1,
            Author = searchPayload.AuthorName,
            Class = string.Empty,
            Difficulty = difficulty, // TODO verify these value match
            Level = searchPayload.LevelName,
            Rating = searchPayload.Rating,
            Sorttype = 2,
            Type = gameEngine,
            DurationClass = duration,
            SortIdx = 8,
            Idx = 0
        };
    }

    private IEnumerable<KeyValuePair<string, string>> ConvertRequest(TrleSearchRequest convertedSearchRequest)
    {
        return ReflectionUtils.GetPropertiesAsKeyValuePairs(convertedSearchRequest, k => k.ToLowerInvariant());
    }
}