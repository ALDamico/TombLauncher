using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class DownloaderViewModel : ObservableObject
{
    [ObservableProperty] private string _baseUrl = string.Empty;
    [ObservableProperty] private string _displayName = string.Empty;
    [ObservableProperty] private bool _isChecked;
    [ObservableProperty] private int _priority;
    [ObservableProperty] private string _className = string.Empty;
    [ObservableProperty] private string _supportedFeatures = string.Empty;
}