using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.ViewModels;

public partial class PageViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isBusy;
}