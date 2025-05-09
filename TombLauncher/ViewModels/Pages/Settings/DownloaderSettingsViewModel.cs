﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
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
    public DownloaderSettingsViewModel(PageViewModel settingsPage) : base("DOWNLOADERS", settingsPage)
    {
        MoveUpCmd = new RelayCommand<DownloaderViewModel>(MoveUp, CanMoveUp);
        MoveDownCmd = new RelayCommand<DownloaderViewModel>(MoveDown, CanMoveDown);
        InfoTipContent = "Downloaders infotip content".GetLocalizedString();
        _settingsService = Ioc.Default.GetRequiredService<SettingsService>();
        _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
        var platformSpecificFeatures = Ioc.Default.GetRequiredService<IPlatformSpecificFeatures>();
        var mapper = new Mapper(Ioc.Default.GetRequiredService<MapperConfiguration>());
        AvailableUnzipFallbackMethods =
            mapper.Map<ObservableCollection<UnzipBackendViewModel>>(platformSpecificFeatures
                .GetPlatformSpecificZipFallbackPrograms()) ?? new ObservableCollection<UnzipBackendViewModel>();
        SelectedUnzipFallbackMethod =
            AvailableUnzipFallbackMethods!.FirstOrDefault(m => m.Name == _settingsService.GetUnzipFallbackMethod())!;
        if (SelectedUnzipFallbackMethod == null)
            SelectedUnzipFallbackMethod = AvailableUnzipFallbackMethods.FirstOrDefault()!;
        CleanUpTempFilesCmd = new AsyncRelayCommand(CleanUpTempFiles);
    }

    [ObservableProperty] private ObservableCollection<DownloaderViewModel> _availableDownloaders;
    [ObservableProperty] private DownloaderViewModel _selectedDownloader;
    [ObservableProperty] private ObservableCollection<UnzipBackendViewModel> _availableUnzipFallbackMethods;
    [ObservableProperty] private UnzipBackendViewModel _selectedUnzipFallbackMethod;
    private readonly SettingsService _settingsService;
    private readonly IMessageBoxService _messageBoxService;

    public ICommand MoveUpCmd { get; }

    private void MoveUp(DownloaderViewModel downloaderViewModel)
    {
        var targetPriority = --downloaderViewModel.Priority;
        if (targetPriority < 0)
            targetPriority = 0;
        var prioritiesToBump = AvailableDownloaders.Where(dl => dl.Priority == targetPriority);
        AvailableDownloaders.Move(downloaderViewModel.Priority, targetPriority);
        foreach (var downloader in prioritiesToBump)
        {
            downloader.Priority++;
        }


        downloaderViewModel.Priority = targetPriority;
        RaiseCanExecuteChanged<DownloaderViewModel>(MoveUpCmd);
        RaiseCanExecuteChanged<DownloaderViewModel>(MoveDownCmd);
    }

    private bool CanMoveUp(DownloaderViewModel downloaderViewModel)
    {
        if (downloaderViewModel == null) return false;
        return downloaderViewModel.Priority > 1;
    }

    public ICommand MoveDownCmd { get; }

    private void MoveDown(DownloaderViewModel downloaderViewModel)
    {
        var targetPriority = ++downloaderViewModel.Priority;
        var prioritiesToBump = AvailableDownloaders.Where(dl => dl.Priority == targetPriority);
        AvailableDownloaders.Move(downloaderViewModel.Priority, targetPriority);
        foreach (var downloader in prioritiesToBump)
        {
            downloader.Priority--;
        }

        downloaderViewModel.Priority = targetPriority;
        RaiseCanExecuteChanged<DownloaderViewModel>(MoveUpCmd);
        RaiseCanExecuteChanged<DownloaderViewModel>(MoveDownCmd);
    }

    private bool CanMoveDown(DownloaderViewModel downloaderViewModel)
    {
        if (downloaderViewModel == null) return false;
        return downloaderViewModel.Priority < AvailableDownloaders.Count;
    }
    
    public ICommand CleanUpTempFilesCmd { get; }

    private async Task CleanUpTempFiles()
    {
        await _settingsService.CleanUpTempFiles();
        await _messageBoxService.ShowLocalized("Clean up completed!", "The clean up process has completed successfully!",
            MsgBoxButton.Ok, MsgBoxImage.Information);
    }
}