using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class DailyStatisticsViewModel : ObservableObject
{
    [ObservableProperty] private DateTime _date;
    [ObservableProperty] private int _differentGamesPlayed;
    [ObservableProperty] private TimeSpan _averageGameDuration;
    [ObservableProperty] private TimeSpan _totalPlayTime;
}