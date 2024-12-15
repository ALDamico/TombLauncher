using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class DownloaderViewModel : ObservableObject
{
    [ObservableProperty] private string _baseUrl;
    [ObservableProperty] private string _displayName;
    [ObservableProperty] private bool _isEnabled;
    [ObservableProperty] private int _priority;
    [ObservableProperty] private string _className;
    [ObservableProperty] private string _supportedFeatures;
}