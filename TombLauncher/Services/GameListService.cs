using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.Navigation;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Localization.Extensions;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Dialogs;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class GameListService : IViewService
{
    public GameListService(ViewServiceContext viewContext,
        GamesUnitOfWork gamesUnitOfWork,
        ISettingsProvider settingsProvider)
    {
        ViewContext = viewContext;
        _gamesUnitOfWork = gamesUnitOfWork;
        _settingsProvider = settingsProvider;
    }

    public ViewServiceContext ViewContext { get; }
    private readonly GamesUnitOfWork _gamesUnitOfWork;
    public ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;
    public IMessageBoxService MessageBoxService => ViewContext.MessageBoxService;
    public IDialogService DialogService => ViewContext.DialogService;
    private IMapper _mapper => ViewContext.Mapper;
    private readonly ISettingsProvider _settingsProvider;

    public async Task<ObservableCollection<GameWithStatsViewModel>> FetchGames(GameListViewModel host)
    {
        host.SetBusy(true, "LOADING_GAMES".GetLocalizedString());

        var gamesWithStats = await _gamesUnitOfWork.GetGamesWithStats(true);

        return _mapper.Map<ObservableCollection<GameWithStatsViewModel>>(gamesWithStats);
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
            target.SetBusy(true, "UNINSTALLING".GetLocalizedString(game.GameMetadata.Title));
            var installDir = game.GameMetadata.InstallDirectory;
            if (installDir != null)
                Directory.Delete(installDir, true);
            _gamesUnitOfWork.MarkGameAsUninstalled(game.GameMetadata.Id);
            await _gamesUnitOfWork.Save();
            target.ClearBusy();
            // Refresh logic:
            await NavigationManager.NavigateTo<GameListViewModel>();
        };
        DialogService.ShowDialog(confirmDialogViewModel, _ => { });
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