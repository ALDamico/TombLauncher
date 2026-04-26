using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using TombLauncher.Contracts.Localization;
using TombLauncher.Data.Database.Services;
using TombLauncher.Localization.Extensions;
using TombLauncher.Mappers;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Dialogs;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class GameListService : IViewService
{
    public GameListService(ViewServiceContext viewContext,
        GameDataService gameDataService,
        ISettingsProvider settingsProvider,
        GameMetadataMapper gameMetadataMapper,
        GameWithStatsService gameWithStatsService)
    {
        ViewContext = viewContext;
        _gameDataService = gameDataService;
        _settingsProvider = settingsProvider;
        _gameMetadataMapper = gameMetadataMapper;
        _gameWithStatsService = gameWithStatsService;
    }

    public ViewServiceContext ViewContext { get; }
    private readonly GameDataService _gameDataService;
    public ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;
    private readonly ISettingsProvider _settingsProvider;
    private readonly GameMetadataMapper _gameMetadataMapper;
    private readonly GameWithStatsService _gameWithStatsService;

    public async Task<ObservableCollection<GameWithStatsViewModel>> FetchGames(GameListViewModel host)
    {
        using (host.BusyScope("LOADING_GAMES".GetLocalizedString()))
        {
            var gamesWithStats = await _gameDataService.GetGamesWithStats(true);
            return _gameMetadataMapper.ToObservableCollection(gamesWithStats, _gameWithStatsService);
        }
    }

    public async Task AddGame()
    {
        await NavigationManager.NavigateTo<NewGameViewModel>();
    }

    public async Task Uninstall(GameListViewModel target, GameWithStatsViewModel game)
    {
        var confirmDialogViewModel = new GameUninstallConfirmDialogViewModel() { Game = game.GameMetadata };
        confirmDialogViewModel.RequestCloseDialog += async (_, args) =>
        {
            if (!args.DialogResult) return;
            using (target.BusyScope("UNINSTALLING".GetLocalizedString(game.GameMetadata.Title)))
            {
                var installDir = game.GameMetadata.InstallDirectory;
                if (installDir != null)
                    Directory.Delete(installDir, true);
                await _gameDataService.MarkGameAsUninstalled(game.GameMetadata.Id);
            }
            // Refresh logic:
            await NavigationManager.NavigateTo<GameListViewModel>();
        };
        ViewContext.PopupService.ShowDialog(confirmDialogViewModel, _ => { });
        await Task.CompletedTask;
    }

    public async Task OpenSearch()
    {
        await NavigationManager.NavigateTo<GameSearchViewModel>();
    }

    public void ApplySettings(GameListViewModel target)
    {
        target.ShowAsGrid = _settingsProvider.GetAppearanceSettings().IsGridViewDefault;
    }
}