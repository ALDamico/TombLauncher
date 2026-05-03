using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Kernel;

namespace TombLauncher.ViewModels;

public partial class DailyGameDurationViewModel : ObservableObject, IChartEntity
{
    [ObservableProperty] private DateTime _date;
    [ObservableProperty] private TimeSpan _gameDuration;
    public ChartEntityMetaData? MetaData { get; set; }
    public Coordinate Coordinate { get; set; }
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        Coordinate = new Coordinate(Date.Ticks, GameDuration.Ticks);
        base.OnPropertyChanged(e);
    }
}