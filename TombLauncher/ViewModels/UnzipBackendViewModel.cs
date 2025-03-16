using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class UnzipBackendViewModel : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _command;
    [ObservableProperty] private string _commandLineArguments;
}