using System.Collections.Generic;
using System.Threading.Tasks;
using TombLauncher.ViewModels;

namespace TombLauncher.Installers.Downloaders;

public interface IGameDownloader
{
    string BaseUrl { get; }
    Task<List<GameMetadataViewModel>> GetGames(DownloaderSearchPayload searchPayload);
}