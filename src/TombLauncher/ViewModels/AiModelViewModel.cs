using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Ai.Services;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Dtos;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels;

public partial class AiModelViewModel : ObservableObject
{
    private readonly ModelDownloadService _modelDownloadService;
    private readonly NotificationService _notificationService;

    public AiModelViewModel(AiModelMetadata metadata, ModelDownloadService modelDownloadService, NotificationService notificationService)
    {
        _modelDownloadService = modelDownloadService;
        _notificationService = notificationService;
        Metadata = metadata;
        DownloadCmd = new AsyncRelayCommand(Download, CanDownload);
        CancelDownloadCmd = new AsyncRelayCommand(CancelDownload, CanCancelDownload);
    }

    [ObservableProperty] private AiModelMetadata _metadata;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private long? _fileSizeBytes;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelDownloadCmd))]
    [NotifyCanExecuteChangedFor(nameof(DownloadCmd))]
    private bool _isDownloaded;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelDownloadCmd))]
    [NotifyCanExecuteChangedFor(nameof(DownloadCmd))]
    private bool _isDownloading;

    [ObservableProperty] private InstallProgressViewModel? _installProgress;
    [ObservableProperty] private bool _isFetchingSize;
    public IAsyncRelayCommand DownloadCmd { get; }

    private async Task Download()
    {
        InstallProgress = new InstallProgressViewModel();
        IsDownloading = true;
        var notification = new NotificationViewModel()
        {
            Content = InstallProgress, CancelCommand = CancelDownloadCmd,
            Type = NotificationType.Info,
            Title = "DOWNLOADING_MODEL_TITLE".GetLocalizedString(), IsDismissable = false, IsOpenable = false
        };
        await _notificationService.AddNotificationAsync(notification);
        try
        {
            await _modelDownloadService.DownloadAsync(Metadata, new Progress<DownloadProgressInfo>(d =>
            {
                if (InstallProgress == null)
                    return;
                InstallProgress.CurrentBytes = d.BytesDownloaded;
                InstallProgress.TotalBytes = d.TotalBytes;
                InstallProgress.ProcessStarted = true;
                InstallProgress.DownloadSpeed = d.DownloadSpeed;
                InstallProgress.InstallStatus = InstallStatus.Downloading;
                InstallProgress.Message =
                    "DOWNLOADING_MODEL".GetLocalizedString(Metadata.FriendlyName, Metadata.DownloadLink);
            }), _cancellationTokenSource.Token);
            notification.Type = NotificationType.Success;
            IsDownloaded = true;
        }
        catch (OperationCanceledException)
        {
            notification.Type = NotificationType.Warning;
            notification.Title = "MODEL_DOWNLOAD_CANCELED".GetLocalizedString(Metadata.FriendlyName);
        }
        catch (IOException ex)
        {
            await _notificationService.AddErrorNotificationAsync("ERROR_DOWNLOADING_MODEL", ex.Message,
                PackIconRemixIconKind.SignalWifiErrorLine);
            notification.Type = NotificationType.Error;
            notification.IsDismissable = true;
            notification.IsCancelable = false;
        }
        finally
        {
            IsDownloading = false;
        }
    }

    private bool CanDownload() => !(IsDownloading || IsDownloaded);

    public IAsyncRelayCommand CancelDownloadCmd { get; }

    private async Task CancelDownload()
    {
        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new();
    }

    private bool CanCancelDownload() => IsDownloading;

    private CancellationTokenSource _cancellationTokenSource = new();
}