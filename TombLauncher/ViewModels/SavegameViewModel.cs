﻿using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TombLauncher.ViewModels;

public partial class SavegameViewModel : ViewModelBase
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _filename;
    [ObservableProperty] private int _slotNumber;
    [ObservableProperty] private int _saveNumber;
    [ObservableProperty] private string _levelName;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(DeleteSavegameCmd))] private bool _isStartOfLevel;
    [ObservableProperty] private DateTime? _backedUpOn;
    [ObservableProperty] private long _length;
    public ICommand UpdateStartOfLevelStateCmd { get; set; }
    public IRelayCommand DeleteSavegameCmd { get; set; }
    public ICommand RestoreSavegameCmd { get; set; }
}