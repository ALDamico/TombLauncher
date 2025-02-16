using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Contracts.Localization;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Localization.Extensions;
using TombLauncher.Navigation;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Dialogs;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class GameListService : IViewService
{
    public GameListService(ILocalizationManager localizationManager,
        NavigationManager navigationManager, 
        IMessageBoxService messageBoxService, 
        IDialogService dialogService)
    {
        _gamesUnitOfWork = Ioc.Default.GetRequiredService<GamesUnitOfWork>();
        LocalizationManager = localizationManager;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
        _mapper = Ioc.Default.GetRequiredService<MapperConfiguration>().CreateMapper();
    }

    private readonly GamesUnitOfWork _gamesUnitOfWork;
    public ILocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }
    private readonly IMapper _mapper;

    public async Task<ObservableCollection<GameWithStatsViewModel>> FetchGames(GameListViewModel host)
    {
        host.SetBusy(true, "Loading games...".GetLocalizedString());

        var gamesWithStats = await _gamesUnitOfWork.GetGamesWithStats(true);

        return _mapper.Map<ObservableCollection<GameWithStatsViewModel>>(gamesWithStats);
    }

    public void AddGame()
    {
        var newGameViewModel = Ioc.Default.GetService<NewGameViewModel>();
        NavigationManager.NavigateTo(newGameViewModel);
    }

    public async Task Uninstall(GameListViewModel target, GameWithStatsViewModel game)
    {
        var confirmDialogViewModel = new GameUninstallConfirmDialogViewModel() { Game = game.GameMetadata };
        confirmDialogViewModel.RequestCloseDialog += async (_, args) =>
        {
            if (!args.DialogResult) return;
            target.SetBusy(true, "Uninstalling".GetLocalizedString(game.GameMetadata.Title));
            var installDir = game.GameMetadata.InstallDirectory;
            Directory.Delete(installDir, true);
            _gamesUnitOfWork.MarkGameAsUninstalled(game.GameMetadata.Id);
            await _gamesUnitOfWork.Save();
            target.ClearBusy();
            await NavigationManager.NavigateTo(target);
        };
        DialogService.ShowDialog(confirmDialogViewModel, _ => { });
        await Task.CompletedTask;
    }

    public async Task OpenSearch()
    {
        await NavigationManager.NavigateTo(Task.FromResult<PageViewModel>(Ioc.Default.GetRequiredService<GameSearchViewModel>()));
    }
}