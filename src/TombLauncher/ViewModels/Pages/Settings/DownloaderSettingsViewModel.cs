using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.RemixIcon;

using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Configuration;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Extensions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class DownloaderSettingsViewModel : SettingsSectionViewModelBase
{
    public DownloaderSettingsViewModel(PageViewModel settingsPage, ISettingsProvider settingsProvider, IAppFileOperationsService appFileOperations, IPopupService popupService, IPlatformSpecificFeatures platformSpecificFeatures, MapperConfiguration mapperConfiguration) : base("DOWNLOADERS", settingsPage, PackIconRemixIconKind.DownloadLine)
    {
        InfoTipContent = "DOWNLOADERS_INFOTIP_CONTENT".GetLocalizedString();
        _settingsProvider = settingsProvider;
        _appFileOperations = appFileOperations;
        _popupService = popupService;
        _mapper = mapperConfiguration.CreateMapper();
        AvailableUnzipFallbackMethods =
            _mapper.Map<ObservableCollection<UnzipBackendViewModel>>(platformSpecificFeatures
                .GetPlatformSpecificZipFallbackPrograms()) ?? new ObservableCollection<UnzipBackendViewModel>();
        SelectedUnzipFallbackMethod =
            AvailableUnzipFallbackMethods!.FirstOrDefault(m => m.Name == _settingsProvider.GetGameDetailsSettings().UnzipFallbackMethod)!;
        if (SelectedUnzipFallbackMethod == null)
            SelectedUnzipFallbackMethod = AvailableUnzipFallbackMethods.FirstOrDefault()!;
    }

    [ObservableProperty] private ObservableCollection<DownloaderViewModel> _availableDownloaders = [];

    partial void OnAvailableDownloadersChanged(ObservableCollection<DownloaderViewModel>? oldValue, ObservableCollection<DownloaderViewModel> newValue)
    {
        if (oldValue != null)
            foreach (var item in oldValue)
                item.PropertyChanged -= OnDownloaderPropertyChanged;

        foreach (var item in newValue)
            item.PropertyChanged += OnDownloaderPropertyChanged;
    }

    private void OnDownloaderPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(AvailableDownloaders));
    }
    [ObservableProperty]
    [IgnoreChanges]
    private DownloaderViewModel? _selectedDownloader;
    [ObservableProperty] private ObservableCollection<UnzipBackendViewModel> _availableUnzipFallbackMethods;
    [ObservableProperty] private UnzipBackendViewModel _selectedUnzipFallbackMethod;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IAppFileOperationsService _appFileOperations;
    private readonly IPopupService _popupService;
    private readonly IMapper _mapper;

    public void Reorder(int oldIndex, int newIndex)
    {
        if (oldIndex == newIndex) return;
        if (oldIndex < 0 || oldIndex >= AvailableDownloaders.Count) return;
        if (newIndex < 0 || newIndex >= AvailableDownloaders.Count) return;

        AvailableDownloaders.Move(oldIndex, newIndex);
        for (var i = 0; i < AvailableDownloaders.Count; i++)
        {
            AvailableDownloaders[i].Priority = i + 1;
        }

        OnPropertyChanged(nameof(AvailableDownloaders));
    }

    [RelayCommand]
    private async Task CleanUpTempFiles()
    {
        await _appFileOperations.CleanUpTempFiles();
        await _popupService.ShowLocalized("CLEAN_UP_COMPLETED", "THE_CLEAN_UP_PROCESS_HAS_COMPLETED_SUCCESSFULLY",
            MsgBoxButton.Ok, MsgBoxImage.Information);
    }

    public override void ApplyTo(AppConfiguration userConfig)
    {
        userConfig.Downloaders.Sources = _mapper.Map<List<DownloaderConfiguration>>(AvailableDownloaders);
        userConfig.Downloaders.UnzipFallbackMethod = SelectedUnzipFallbackMethod?.Name;
    }
}