using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.ViewModels;

public partial class InstallProgressViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial double TotalBytes { get; set; }

    [ObservableProperty]
    public partial double CurrentBytes { get; set; }

    [ObservableProperty]
    public partial double DownloadSpeed { get; set; }

    [ObservableProperty]
    public partial double InstallPercentage { get; set; }

    [ObservableProperty]
    public partial string CurrentFileName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    [ObservableProperty]
    public partial InstallStatus InstallStatus { get; set; }

    [ObservableProperty]
    public partial bool ProcessStarted { get; set; } = true;
}