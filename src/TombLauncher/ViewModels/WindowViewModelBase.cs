using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class WindowViewModelBase : ViewModelBase
{
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;
}