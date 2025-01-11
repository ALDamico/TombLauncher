using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class DayOfWeekStatisticsViewModel : ObservableObject
{
    [ObservableProperty] private DayOfWeek _dayOfWeek;
    [ObservableProperty] private TimeSpan _averageTimePlayed;
    [ObservableProperty] private int _playSessionsCount;
}