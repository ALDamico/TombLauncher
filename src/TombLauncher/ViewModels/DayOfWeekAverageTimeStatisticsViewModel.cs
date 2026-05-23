using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Kernel;

namespace TombLauncher.ViewModels;

public partial class DayOfWeekAverageTimeStatisticsViewModel : ObservableObject, IChartEntity
{
    [ObservableProperty]
    public partial DayOfWeek DayOfWeek { get; set; }

    [ObservableProperty]
    public partial TimeSpan AverageTimePlayed { get; set; }
    [ObservableProperty]
    public partial int Index { get; set; }
    public ChartEntityMetaData? MetaData { get; set; }
    public Coordinate Coordinate { get; set; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        Coordinate = new Coordinate(Index, AverageTimePlayed.Ticks);
        base.OnPropertyChanged(e);
    }
}