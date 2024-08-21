using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Database.Entities;
using TombLauncher.Database.UnitOfWork;
using TombLauncher.ViewModels.Extensions;

namespace TombLauncher.ViewModels.ViewModels;

public partial class NewGameViewModel : PageViewModel
{
    public NewGameViewModel(GamesUnitOfWork gamesUnitOfWork)
    {
        _gamesUoW = gamesUnitOfWork;
    }

    private GamesUnitOfWork _gamesUoW;
    [ObservableProperty] private GameMetadataViewModel _gameMetadata;
    [ObservableProperty] private string _source;
    
    public ICommand PickZipArchiveCmd { get; }

    private void PickZipArchive()
    {
        TopLevel
    }

    protected override void SaveInner()
    {
        IsBusy = true;
        _gamesUoW.UpsertGame(GameMetadata.ToDto());

        IsBusy = false;
    }
}