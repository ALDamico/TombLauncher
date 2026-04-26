using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Kernel;

namespace TombLauncher.ViewModels;

public partial class DayOfWeekAverageTimeStatisticsViewModel : ObservableObject, IChartEntity
{
    [ObservableProperty] private DayOfWeek _dayOfWeek;
    [ObservableProperty] private TimeSpan _averageTimePlayed;
    [ObservableProperty] private int _index;
    public ChartEntityMetaData? MetaData { get; set; }
    public Coordinate Coordinate { get; set; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        Coordinate = new Coordinate(Index, AverageTimePlayed.Ticks);
        base.OnPropertyChanged(e);
    }
}