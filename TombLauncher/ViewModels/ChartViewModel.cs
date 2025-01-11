using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace TombLauncher.ViewModels;

public partial class ChartViewModel : ObservableObject
{
    [ObservableProperty] private Axis[] _xAxis;
    [ObservableProperty] private Axis[] _yAxis;
    [ObservableProperty] private ISeries[] _series;
}