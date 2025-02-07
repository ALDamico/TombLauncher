using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class SavegameSlotViewModel : ObservableObject
{
    [ObservableProperty] private string _header;
    [ObservableProperty] private int? _saveSlot;
    [ObservableProperty] private bool _isEnabled;
    public ICommand FilterCmd { get; set; }
}