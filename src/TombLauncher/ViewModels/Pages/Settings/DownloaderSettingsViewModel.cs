using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Extensions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class DownloaderSettingsViewModel : SettingsSectionViewModelBase
{
    public DownloaderSettingsViewModel(PageViewModel settingsPage, ISettingsProvider settingsProvider, IAppFileOperationsService appFileOperations, IMessageBoxService messageBoxService, IPlatformSpecificFeatures platformSpecificFeatures, MapperConfiguration mapperConfiguration) : base("DOWNLOADERS", settingsPage)
    {
        InfoTipContent = "DOWNLOADERS_INFOTIP_CONTENT".GetLocalizedString();
        _settingsProvider = settingsProvider;
        _appFileOperations = appFileOperations;
        _messageBoxService = messageBoxService;
        var mapper = new Mapper(mapperConfiguration);
        AvailableUnzipFallbackMethods =
            mapper.Map<ObservableCollection<UnzipBackendViewModel>>(platformSpecificFeatures
                .GetPlatformSpecificZipFallbackPrograms()) ?? new ObservableCollection<UnzipBackendViewModel>();
        SelectedUnzipFallbackMethod =
            AvailableUnzipFallbackMethods!.FirstOrDefault(m => m.Name == _settingsProvider.GetGameDetailsSettings().UnzipFallbackMethod)!;
        if (SelectedUnzipFallbackMethod == null)
            SelectedUnzipFallbackMethod = AvailableUnzipFallbackMethods.FirstOrDefault()!;
    }

    [ObservableProperty] private ObservableCollection<DownloaderViewModel> _availableDownloaders = new ObservableCollection<DownloaderViewModel>();
    [ObservableProperty] private DownloaderViewModel? _selectedDownloader;
    [ObservableProperty] private ObservableCollection<UnzipBackendViewModel> _availableUnzipFallbackMethods;
    [ObservableProperty] private UnzipBackendViewModel _selectedUnzipFallbackMethod;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IAppFileOperationsService _appFileOperations;
    private readonly IMessageBoxService _messageBoxService;

    [RelayCommand(CanExecute = nameof(CanMoveUp))]
    private void MoveUp(DownloaderViewModel? downloaderViewModel)
    {
        if (downloaderViewModel == null) return;
        var currentIndex = AvailableDownloaders.IndexOf(downloaderViewModel);
        var targetIndex = currentIndex - 1;
        if (targetIndex < 0)
            return;
        AvailableDownloaders.Move(currentIndex, targetIndex);
        // Recalculate priorities based on position
        for (var i = 0; i < AvailableDownloaders.Count; i++)
        {
            AvailableDownloaders[i].Priority = i + 1;
        }

        MoveUpCommand.NotifyCanExecuteChanged();
        MoveDownCommand.NotifyCanExecuteChanged();
    }

    private bool CanMoveUp(DownloaderViewModel? downloaderViewModel)
    {
        if (downloaderViewModel == null) return false;
        return downloaderViewModel.Priority > 1;
    }

    [RelayCommand(CanExecute = nameof(CanMoveDown))]
    private void MoveDown(DownloaderViewModel? downloaderViewModel)
    {
        if (downloaderViewModel == null) return;
        var currentIndex = AvailableDownloaders.IndexOf(downloaderViewModel);
        var targetIndex = currentIndex + 1;
        if (targetIndex >= AvailableDownloaders.Count)
            return;
        AvailableDownloaders.Move(currentIndex, targetIndex);
        // Recalculate priorities based on position
        for (var i = 0; i < AvailableDownloaders.Count; i++)
        {
            AvailableDownloaders[i].Priority = i + 1;
        }

        MoveUpCommand.NotifyCanExecuteChanged();
        MoveDownCommand.NotifyCanExecuteChanged();
    }

    private bool CanMoveDown(DownloaderViewModel? downloaderViewModel)
    {
        if (downloaderViewModel == null) return false;
        return downloaderViewModel.Priority < AvailableDownloaders.Count;
    }

    [RelayCommand]
    private async Task CleanUpTempFiles()
    {
        await _appFileOperations.CleanUpTempFiles();
        await _messageBoxService.ShowLocalized("CLEAN_UP_COMPLETED", "THE_CLEAN_UP_PROCESS_HAS_COMPLETED_SUCCESSFULLY",
            MsgBoxButton.Ok, MsgBoxImage.Information);
    }
}