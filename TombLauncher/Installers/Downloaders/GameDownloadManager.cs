using System.Collections.Generic;
using System.Threading.Tasks;
using TombLauncher.ViewModels;

namespace TombLauncher.Installers.Downloaders;

public class GameDownloadManager
{
    public GameDownloadManager()
    {
        Downloaders = new List<IGameDownloader>();
    }
    public List<IGameDownloader> Downloaders { get; }

    public async Task<List<GameSearchResultMetadataViewModel>> GetGames(DownloaderSearchPayload searchPayload)
    {
        var outputList = new List<GameSearchResultMetadataViewModel>();
        foreach (var downloader in Downloaders)
        {
            var gamesByDownloader = await downloader.GetGames(searchPayload);
            // TODO Merge games somehow
            outputList.AddRange(gamesByDownloader);
        }

        return outputList;
    }
}