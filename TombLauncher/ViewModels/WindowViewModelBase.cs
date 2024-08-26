using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class WindowViewModelBase : ViewModelBase
{
    [ObservableProperty] private string _title;
}