using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class SavegameSlotViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Header { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int? SaveSlot { get; set; }

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }
    public ICommand? FilterCommand { get; set; }
}