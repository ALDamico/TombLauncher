using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels;

public partial class DownloaderSearchPayloadViewModel : ObservableValidator
{
    public DownloaderSearchPayloadViewModel()
    {
        AvailableDifficulties = EnumUtils.GetEnumViewModels<GameDifficulty>().ToObservableCollection();
        AvailableEngines = EnumUtils.GetEnumViewModels<GameEngine>().ToObservableCollection();
    }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    public partial string LevelName { get; set; } = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    public partial string AuthorName { get; set; } = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    public partial GameEngine? GameEngine { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    public partial GameDifficulty? GameDifficulty { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    public partial GameLength? Duration { get; set; }

    [ObservableProperty]
    [Range(0, 10)]
    [NotifyPropertyChangedFor(nameof(HasActiveFilters))]
    public partial int Rating { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<EnumViewModel<GameDifficulty>> AvailableDifficulties { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<EnumViewModel<GameEngine>> AvailableEngines { get; set; }

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(LevelName) ||
        !string.IsNullOrWhiteSpace(AuthorName) ||
        GameEngine != null ||
        GameDifficulty != null ||
        Duration != null ||
        Rating != 0;

    public void ClearFilters()
    {
        LevelName = string.Empty;
        AuthorName = string.Empty;
        GameEngine = null;
        GameDifficulty = null;
        Duration = null;
        Rating = 0;
    }
}