using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class DownloaderSettings : ViewModelBase
{
    public DownloaderSettings()
    {
        MoveUpCmd = new RelayCommand<DownloaderViewModel>(MoveUp, CanMoveUp);
        MoveDownCmd = new RelayCommand<DownloaderViewModel>(MoveDown, CanMoveDown);
    }
    [ObservableProperty] private ObservableCollection<DownloaderViewModel> _availableDownloaders;
    [ObservableProperty] private DownloaderViewModel _selectedDownloader;

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
}