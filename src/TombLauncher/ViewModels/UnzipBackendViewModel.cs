using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class UnzipBackendViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _command = string.Empty;
    [ObservableProperty] private string _commandLineArguments = string.Empty;
}