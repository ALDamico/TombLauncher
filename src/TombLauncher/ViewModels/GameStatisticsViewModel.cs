using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class GameStatisticsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Duration))]
    public partial DateTime? LastPlayed { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Duration))]
    public partial DateTime? LastPlayedEnd { get; set; }

    [ObservableProperty]
    public partial uint TotalSessions { get; set; }

    [ObservableProperty]
    public partial int Id { get; set; }

    public TimeSpan Duration => LastPlayedEnd.GetValueOrDefault() - LastPlayed.GetValueOrDefault();
}