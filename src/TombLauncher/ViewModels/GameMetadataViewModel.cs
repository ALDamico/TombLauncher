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
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? Author { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime? ReleaseDate { get; set; }

    [ObservableProperty]
    public partial DateTime? InstallDate { get; set; }

    [ObservableProperty]
    public partial GameEngine GameEngine { get; set; }

    [ObservableProperty]
    public partial string? Setting { get; set; }

    [ObservableProperty]
    public partial GameLength Length { get; set; }

    [ObservableProperty]
    public partial GameDifficulty Difficulty { get; set; }

    [ObservableProperty]
    public partial string? InstallDirectory { get; set; }

    [ObservableProperty]
    public partial string? ExecutablePath { get; set; }

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Bitmap? TitlePic { get; set; }

    [ObservableProperty]
    public partial string? AuthorFullName { get; set; }

    [ObservableProperty]
    public partial bool IsInstalled { get; set; }

    [ObservableProperty]
    public partial string? SetupExecutable { get; set; }

    [ObservableProperty]
    public partial string? SetupExecutableArgs { get; set; }

    [ObservableProperty]
    public partial string? CommunitySetupExecutable { get; set; }

    [ObservableProperty]
    public partial bool IsCompleted { get; set; }

    [ObservableProperty]
    public partial bool IsFavourite { get; set; }

    [ObservableProperty]
    public partial string? InstalledFromSiteDisplayName { get; set; }

    [ObservableProperty]
    public partial string? CompatibilityPrefixPath { get; set; }

    [ObservableProperty]
    public partial CompatibilityTool CompatibilityTool { get; set; }

    [ObservableProperty]
    public partial string? CompatibilityToolPath { get; set; }

    [ObservableProperty]
    public partial string? TitlePicUrl { get; set; }

    [ObservableProperty]
    public partial string? InstalledFromLink { get; set; }

    public List<EnvironmentVariableDto> ExtraEnvVars { get; set; } = [];
    public Guid Guid { get; set; }
    
    [ObservableProperty]
    public partial bool EnableBorderlessFix { get; set; }
}