using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.ViewModels;

public partial class InstallProgressViewModel : ViewModelBase
{
    [ObservableProperty] private double _totalBytes;
    [ObservableProperty] private double _currentBytes;
    [ObservableProperty] private double _downloadSpeed;
    [ObservableProperty] private double _installPercentage;
    [ObservableProperty] private string _currentFileName = string.Empty;
    [ObservableProperty] private string _message = string.Empty;
    [ObservableProperty] private InstallStatus _installStatus;
    [ObservableProperty] private bool _processStarted = true;
}