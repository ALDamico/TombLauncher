using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Kernel;

namespace TombLauncher.ViewModels;

public partial class DailyGameDurationViewModel : ObservableObject, IChartEntity
{
    [ObservableProperty]
    public partial DateTime Date { get; set; }

    [ObservableProperty]
    public partial TimeSpan GameDuration { get; set; }
    public ChartEntityMetaData? MetaData { get; set; }
    public Coordinate Coordinate { get; set; }
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        Coordinate = new Coordinate(Date.Ticks, GameDuration.Ticks);
        base.OnPropertyChanged(e);
    }
}