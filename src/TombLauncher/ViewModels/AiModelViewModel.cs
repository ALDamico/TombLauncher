using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Core.Dtos;

namespace TombLauncher.ViewModels;

public partial class AiModelViewModel : ObservableObject
{
    public AiModelViewModel(AiModelMetadata metadata)
    {
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
        // TODO
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