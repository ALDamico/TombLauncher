using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace TombLauncher.ViewModels;

public partial class ChartViewModel : ObservableObject
{
    [ObservableProperty]
    public partial Axis[] XAxis { get; set; } = null!;

    [ObservableProperty]
    public partial Axis[] YAxis { get; set; } = null!;

    [ObservableProperty]
    public partial ISeries[] Series { get; set; } = null!;
}