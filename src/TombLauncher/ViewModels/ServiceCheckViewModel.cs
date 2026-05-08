using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.ViewModels;

public partial class ServiceCheckViewModel : ObservableObject
{
    [ObservableProperty] private string _checkResultMessage = null!;
    [ObservableProperty] private ServiceCheckStatus _status;
}