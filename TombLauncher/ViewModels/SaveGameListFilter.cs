using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class SaveGameListFilter : ObservableObject
{
    [ObservableProperty] private int? _slotNumber;
    [ObservableProperty] private bool _startOfLevelOnly;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(StartOfLevelOnly))
            Console.WriteLine("PropertyChanged!");
        base.OnPropertyChanged(e);
    }
}