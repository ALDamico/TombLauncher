using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TombLauncher.Models;
using TombLauncher.Utils;
using TombLauncher.ViewModels;

namespace TombLauncher.Installers.Downloaders.TRLE.net;

public class TrleGameDownloader : IGameDownloader
{
    public TrleGameDownloader()
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(BaseUrl)
        };
    }

    public string BaseUrl => "https://trle.net";
    private HttpClient _httpClient;

    private readonly Dictionary<GameEngine, string> _gameEngineMapping = new Dictionary<GameEngine, string>()
    {
        { GameEngine.Unknown, string.Empty },
        { GameEngine.TombRaider1, "TR1" },
        { GameEngine.TombRaider2, "TR2" },
        { GameEngine.TombRaider3, "TR3" },
        { GameEngine.TombRaider4, "TR4" },
        { GameEngine.TombRaider5, "TR5" },
        { GameEngine.Ten, "TEN" }
    };

    public async Task<List<GameMetadataViewModel>> GetGames(DownloaderSearchPayload searchPayload)
    {
        var request = ConvertRequest(searchPayload);
        var requestStrng = ConvertRequest(request);
        var urlEncodedContent = new FormUrlEncodedContent(requestStrng);
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/pFind.php");
        requestMessage.Content = urlEncodedContent;
        requestMessage.Headers.Add("Referer", "https://trle.net/pFind.php");
        requestMessage.Headers.Host = "trle.net";
        requestMessage.Headers.Referrer = new Uri("https://trle.net/pFind.php");
        //text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/avif"));
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/apng"));
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/signed-exchange"));
        var formData = new MultipartFormDataContent()
        {
            urlEncodedContent
        };
        requestMessage.Content = urlEncodedContent;


        var response = await _httpClient.SendAsync(requestMessage);
        var content = await response.Content.ReadAsStringAsync();
        
        throw new System.NotImplementedException();
    }

    private TrleSearchRequest ConvertRequest(DownloaderSearchPayload searchPayload)
    {
        int? difficulty = null;
        if (searchPayload.GameDifficulty != null && searchPayload.GameDifficulty != GameDifficulty.Unknown)
        {
            difficulty = (int)searchPayload.GameDifficulty;
        }

        var gameEngine = _gameEngineMapping[searchPayload.GameEngine];
        int? duration = null;
        if (searchPayload.Duration != GameLength.Unknown)
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
            SortIdx = 8
        };
    }

    private IEnumerable<KeyValuePair<string, string>> ConvertRequest(TrleSearchRequest convertedSearchRequest)
    {
        return ReflectionUtils.GetPropertiesAsKeyValuePairs(convertedSearchRequest, k => k.ToLowerInvariant());
    }
}