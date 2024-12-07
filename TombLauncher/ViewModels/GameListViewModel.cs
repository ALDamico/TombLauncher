using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Localization;
using TombLauncher.Navigation;
using TombLauncher.ViewModels.Dialogs;

namespace TombLauncher.ViewModels;

public partial class GameListViewModel : PageViewModel
{
    private readonly GamesUnitOfWork _gamesUnitOfWork;
    private readonly IDialogService _dialogService;
    [ObservableProperty] private ObservableCollection<GameWithStatsViewModel> _games;
    [ObservableProperty] private GameWithStatsViewModel _selectedGame;

    public GameListViewModel(GamesUnitOfWork gamesUoW, NavigationManager navigationManager, LocalizationManager localizationManager, IDialogService dialogService) : base(localizationManager)
    {
        _gamesUnitOfWork = gamesUoW;
        _dialogService = dialogService;
        AddGameCmd = new RelayCommand(AddGame);
        UninstallCmd = new RelayCommand<GameWithStatsViewModel>(Uninstall);
        navigationManager.OnNavigated += OnInit;
    }

    private async void OnInit()
    {
        SetBusy(true,  LocalizationManager.GetLocalizedString("Loading games..."));
        Games =
            _gamesUnitOfWork.GetGamesWithStats().Select(dto =>
                new GameWithStatsViewModel(_gamesUnitOfWork, LocalizationManager)
                {
                    GameMetadata = dto.GameMetadata.ToViewModel(), 
                    LastPlayed = dto.LastPlayed,
                    TotalPlayedTime = dto.TotalPlayTime
                }).ToObservableCollection();
        ClearBusy();
    }
    
    public ICommand AddGameCmd { get; }

    private void AddGame()
    {
        var newGameViewModel = Ioc.Default.GetService<NewGameViewModel>();
        Program.NavigationManager.NavigateTo(newGameViewModel);
    }
    
    public ICommand UninstallCmd { get; }

    private async void Uninstall(GameWithStatsViewModel game)
    {
        var confirmDialogViewModel = new GameUninstallConfirmDialogViewModel(){Game = game.GameMetadata};

        confirmDialogViewModel.RequestCloseDialog += async (sender, args) =>
        {
            if (!args.DialogResult) return;
            IsBusy = true;
            BusyMessage = LocalizationManager.GetLocalizedString("Uninstalling", game.GameMetadata.Title);
            var installDir = game.GameMetadata.InstallDirectory;
            Directory.Delete(installDir, true);
            _gamesUnitOfWork.DeleteGameById(game.GameMetadata.Id);
            _gamesUnitOfWork.Save();
            IsBusy = false;
            OnInit();
        };
        _dialogService.ShowDialog(confirmDialogViewModel, model => { });
        
    }
}