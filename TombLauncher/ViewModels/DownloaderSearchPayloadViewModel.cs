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
    [ObservableProperty] private string _levelName;
    [ObservableProperty] private string _authorName;
    [ObservableProperty] private GameEngine? _gameEngine;
    [ObservableProperty] private GameDifficulty? _gameDifficulty;
    [ObservableProperty] private GameLength? _duration;
    [ObservableProperty][Range(0, 10)] private int _rating;
    [ObservableProperty] private ObservableCollection<EnumViewModel<GameDifficulty>> _availableDifficulties;
    [ObservableProperty] private ObservableCollection<EnumViewModel<GameEngine>> _availableEngines;

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(LevelName) ||
        !string.IsNullOrWhiteSpace(AuthorName) ||
        GameEngine != null ||
        GameDifficulty != null ||
        Duration != null ||
        Rating != 0;

    partial void OnLevelNameChanged(string value) => OnPropertyChanged(nameof(HasActiveFilters));
    partial void OnAuthorNameChanged(string value) => OnPropertyChanged(nameof(HasActiveFilters));
    partial void OnGameEngineChanged(GameEngine? value) => OnPropertyChanged(nameof(HasActiveFilters));
    partial void OnGameDifficultyChanged(GameDifficulty? value) => OnPropertyChanged(nameof(HasActiveFilters));
    partial void OnDurationChanged(GameLength? value) => OnPropertyChanged(nameof(HasActiveFilters));
    partial void OnRatingChanged(int value) => OnPropertyChanged(nameof(HasActiveFilters));

    public void ClearFilters()
    {
        LevelName = null;
        AuthorName = null;
        GameEngine = null;
        GameDifficulty = null;
        Duration = null;
        Rating = 0;
    }
}