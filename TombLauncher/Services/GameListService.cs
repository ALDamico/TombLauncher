using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Data.Dto;
using TombLauncher.Extensions;
using TombLauncher.Localization;
using TombLauncher.Navigation;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Dialogs;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class GameListService : IViewService
{
    public GameListService(GamesUnitOfWork gamesUnitOfWork, 
        LocalizationManager localizationManager,
        NavigationManager navigationManager, 
        IMessageBoxService messageBoxService, 
        IDialogService dialogService)
    {
        GamesUnitOfWork = gamesUnitOfWork;
        LocalizationManager = localizationManager;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
    }

    public GamesUnitOfWork GamesUnitOfWork { get; }
    public LocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }

    public Task<ObservableCollection<GameWithStatsViewModel>> FetchGames(GameListViewModel host)
    {
        host.IsBusy = true;
        host.BusyMessage = LocalizationManager.GetLocalizedString("Loading games...");
        return Task.FromResult(GamesUnitOfWork.GetGamesWithStats().Select(ConvertDto).ToObservableCollection());
    }

    private GameWithStatsViewModel ConvertDto(GameWithStatsDto dto)
    {
        return new GameWithStatsViewModel(Ioc.Default.GetService<GameWithStatsService>())
        {
            GameMetadata = dto.GameMetadata.ToViewModel(),
            LastPlayed = dto.LastPlayed,
            TotalPlayedTime = dto.TotalPlayTime
        };
    }

    public void AddGame()
    {
        var newGameViewModel = Ioc.Default.GetService<NewGameViewModel>();
        NavigationManager.NavigateTo(newGameViewModel);
    }

    public async Task Uninstall(GameListViewModel target, GameWithStatsViewModel game)
    {
        var confirmDialogViewModel = new GameUninstallConfirmDialogViewModel() { Game = game.GameMetadata };
        confirmDialogViewModel.RequestCloseDialog += (_, args) =>
        {
            if (!args.DialogResult) return;
            target.IsBusy = true;
            target.BusyMessage = LocalizationManager.GetLocalizedString("Uninstalling", game.GameMetadata.Title);
            var installDir = game.GameMetadata.InstallDirectory;
            Directory.Delete(installDir, true);
            GamesUnitOfWork.DeleteGameById(game.GameMetadata.Id);
            GamesUnitOfWork.Save();
            target.IsBusy = false;
            NavigationManager.NavigateTo(target);
        };
        DialogService.ShowDialog(confirmDialogViewModel, _ => { });
        await Task.CompletedTask;
    }
}