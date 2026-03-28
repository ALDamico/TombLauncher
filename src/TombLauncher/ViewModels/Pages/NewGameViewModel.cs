using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Extensions;
using TombLauncher.Services;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels.Pages;

public partial class NewGameViewModel : PageViewModel
{
    public NewGameViewModel(NewGameService newGameService)
    {
        _newGameService = newGameService;
        GameMetadata = new GameMetadataViewModel();
        GameMetadata.PropertyChanged += (_, _) => RaiseCanExecuteChanged(SaveCmd);

        AvailableLengths = EnumUtils.GetEnumViewModels<GameLength>().ToObservableCollection();
        AvailableDifficulties = EnumUtils.GetEnumViewModels<GameDifficulty>().ToObservableCollection();

        InstallProgress = new Progress<CopyProgressInfo>(copyProgressInfo =>
        {
            if (copyProgressInfo.Percentage != null)
            {
                PercentageComplete = copyProgressInfo.Percentage;
            }

            CurrentFileName = copyProgressInfo.CurrentFileName ?? string.Empty;
            if (copyProgressInfo.Message != null)
            {
                BusyMessage = copyProgressInfo.Message;
            }
        });

        PickZipArchiveCmd = new AsyncRelayCommand(PickZipArchive);
        PickFolderCmd = new AsyncRelayCommand(PickFolder);
    }

    private readonly NewGameService _newGameService;
    [ObservableProperty] private GameMetadataViewModel _gameMetadata;
    [ObservableProperty] private string _source = string.Empty;
    public ObservableCollection<EnumViewModel<GameLength>> AvailableLengths { get; }
    public ObservableCollection<EnumViewModel<GameDifficulty>> AvailableDifficulties { get; }
    public IProgress<CopyProgressInfo> InstallProgress { get; }

    public ICommand PickZipArchiveCmd { get; }

    private async Task PickZipArchive()
    {
        Source = await _newGameService.PickZipArchive();
    }

    public ICommand PickFolderCmd { get; }

    private async Task PickFolder()
    {
        Source = await _newGameService.PickFolder();
    }

    protected override async Task SaveInner()
    {
        await _newGameService.InstallGame(GameMetadata, InstallProgress, Source);
    }

    protected override bool CanSave()
    {
        return GameMetadata.Title.IsNotNullOrWhiteSpace() && Source.IsNotNullOrWhiteSpace();
    }

    public IPopupService PopupService => _newGameService.ViewContext.PopupService;
}