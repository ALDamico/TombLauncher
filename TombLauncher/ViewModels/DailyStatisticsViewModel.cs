using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Kernel;

namespace TombLauncher.ViewModels;

public partial class DailyStatisticsViewModel : ObservableObject, IChartEntity
{
    [ObservableProperty] private DateTime _date;
    [ObservableProperty] private int _differentGamesPlayed;
    [ObservableProperty] private TimeSpan _averageGameDuration;
    [ObservableProperty] private TimeSpan _totalPlayTime;
    public ChartEntityMetaData MetaData { get; set; }
    public Coordinate Coordinate { get; set; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Date) || e.PropertyName == nameof(AverageGameDuration))
        {
            Coordinate = new Coordinate(Date.Ticks, AverageGameDuration.Ticks);
        }
        base.OnPropertyChanged(e);
    }
}