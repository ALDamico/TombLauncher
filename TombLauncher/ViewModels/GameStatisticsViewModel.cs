using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class GameStatisticsViewModel : ObservableObject
{
    [ObservableProperty] private string _title;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Duration))]
    private DateTime? _lastPlayed;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Duration))]
    private DateTime? _lastPlayedEnd;

    public TimeSpan Duration => LastPlayedEnd.GetValueOrDefault() - LastPlayed.GetValueOrDefault();
    [ObservableProperty] private uint _totalSessions;
}