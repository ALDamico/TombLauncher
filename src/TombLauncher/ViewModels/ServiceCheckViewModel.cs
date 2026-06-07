using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.ViewModels;

public partial class ServiceCheckViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string CheckResultMessage { get; set; } = null!;

    [ObservableProperty]
    public partial ServiceCheckStatus Status { get; set; }
}