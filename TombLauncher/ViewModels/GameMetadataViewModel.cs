using System;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Models.Models;

namespace TombLauncher.ViewModels;

public partial class GameMetadataViewModel : ViewModelBase
{
    public int Id { get; set; }
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _author;
    [ObservableProperty] private DateTime? _releaseDate;
    [ObservableProperty] private DateTime? _installDate;
    [ObservableProperty] private GameEngine _gameEngine;
    [ObservableProperty] private string _setting;
    [ObservableProperty] private GameLength _length;
    [ObservableProperty] private GameDifficulty _difficulty;
    [ObservableProperty] private string _installDirectory;
    [ObservableProperty] private string _executablePath;
    [ObservableProperty] private string _description;
}