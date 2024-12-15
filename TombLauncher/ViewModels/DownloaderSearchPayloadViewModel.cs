using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Contracts.Enums;
using TombLauncher.Data.Models;

namespace TombLauncher.ViewModels;

public partial class DownloaderSearchPayloadViewModel : ObservableValidator
{
    [ObservableProperty] private string _levelName;
    [ObservableProperty] private string _authorName;
    [ObservableProperty] private GameEngine? _gameEngine;
    [ObservableProperty] private GameDifficulty? _gameDifficulty;
    [ObservableProperty] private GameLength? _duration;
    [ObservableProperty][Range(0, 10)] private int _rating;
}