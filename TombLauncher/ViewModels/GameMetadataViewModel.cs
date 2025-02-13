using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Contracts.Enums;
using TombLauncher.Data.Models;

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
    [ObservableProperty] private Bitmap _titlePic;
    [ObservableProperty] private string _authorFullName;
    [ObservableProperty] private bool _isInstalled;
    [ObservableProperty] private string _setupExecutable;
    [ObservableProperty] private string _setupExecutableArgs;
    [ObservableProperty] private string _communitySetupExecutable;
    public Guid Guid { get; set; }
}