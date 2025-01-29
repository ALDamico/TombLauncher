using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class SavegameViewModel : ViewModelBase
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _filename;
    [ObservableProperty] private int _slotNumber;
    [ObservableProperty] private int _saveNumber;
    [ObservableProperty] private string _levelName;
    [ObservableProperty] private bool _isStartOfLevel;
    [ObservableProperty] private DateTime? _backedUpOn;
    [ObservableProperty] private long _length;
    public ICommand UpdateStartOfLevelStateCmd { get; set; }
    public ICommand DeleteSavegameCmd { get; set; }
    public ICommand RestoreSavegameCmd { get; set; }
}