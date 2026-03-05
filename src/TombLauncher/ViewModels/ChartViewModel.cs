using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace TombLauncher.ViewModels;

public partial class ChartViewModel : ObservableObject
{
    [ObservableProperty] private Axis[] _xAxis = null!;
    [ObservableProperty] private Axis[] _yAxis = null!;
    [ObservableProperty] private ISeries[] _series = null!;
}