using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;

namespace TombLauncher.ViewModels;

public partial class GameMetadataViewModel : ViewModelBase, IGameMetadataLite
{
    public int Id { get; set; }
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _author = string.Empty;
    [ObservableProperty] private DateTime? _releaseDate;
    [ObservableProperty] private DateTime? _installDate;
    [ObservableProperty] private GameEngine _gameEngine;
    [ObservableProperty] private string? _setting;
    [ObservableProperty] private GameLength _length;
    [ObservableProperty] private GameDifficulty _difficulty;
    [ObservableProperty] private string? _installDirectory;
    [ObservableProperty] private string? _executablePath;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private Bitmap? _titlePic;
    [ObservableProperty] private string? _authorFullName;
    [ObservableProperty] private bool _isInstalled;
    [ObservableProperty] private string? _setupExecutable;
    [ObservableProperty] private string? _setupExecutableArgs;
    [ObservableProperty] private string? _communitySetupExecutable;
    [ObservableProperty] private bool _isCompleted;
    [ObservableProperty] private bool _isFavourite;
    [ObservableProperty] private string? _installedFromSiteDisplayName;
    [ObservableProperty] private string? _compatibilityPrefixPath;
    [ObservableProperty] private CompatibilityTool _compatibilityTool;
    [ObservableProperty] private string? _compatibilityToolPath;
    [ObservableProperty] private string? _titlePicUrl;
    [ObservableProperty] private string? _installedFromLink;
    public List<EnvironmentVariableDto> ExtraEnvVars { get; set; } = [];
    public Guid Guid { get; set; }
}