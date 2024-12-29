using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class InstallProgressViewModel: ViewModelBase
{
    [ObservableProperty] private double _totalBytes;
    [ObservableProperty] private double _currentBytes;
    [ObservableProperty] private double _downloadSpeed;
    [ObservableProperty] private double _installPercentage;
    [ObservableProperty] private string _currentFileName;
    [ObservableProperty] private string _message;
    [ObservableProperty] private bool _isDownloading;
    [ObservableProperty] private bool _isInstalling;
}