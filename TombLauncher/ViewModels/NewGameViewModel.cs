using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Database.UnitOfWork;
using TombLauncher.Extensions;

namespace TombLauncher.ViewModels;

public partial class NewGameViewModel : PageViewModel
{
    public NewGameViewModel(GamesUnitOfWork gamesUnitOfWork, IDialogService dialogService)
    {
        _gamesUoW = gamesUnitOfWork;
        _dialogService = dialogService;
        _gameMetadata = new GameMetadataViewModel();

        PickZipArchiveCmd = new RelayCommand(PickZipArchive);
    }

    private GamesUnitOfWork _gamesUoW;
    private readonly IDialogService _dialogService;
    [ObservableProperty] private GameMetadataViewModel _gameMetadata;
    [ObservableProperty] private string _source;
    
    public ICommand PickZipArchiveCmd { get; }

    private async void PickZipArchive()
    {
        var file = await _dialogService.OpenFile("Select a ZIP file", new List<FilePickerFileType>()
        {
            new FilePickerFileType("ZIP files")
            {
                Patterns = new List<string>() { "*.zip" }
            }
        });
        if (string.IsNullOrWhiteSpace(file))
        {
            return;
        }

        Source = file;
    }

    protected override void SaveInner()
    {
        IsBusy = true;
        _gamesUoW.UpsertGame(GameMetadata.ToDto());

        IsBusy = false;
    }
}