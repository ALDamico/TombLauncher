using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels;

public partial class MultiSourceGameSearchResultMetadataViewModel : ViewModelBase
{
    public MultiSourceGameSearchResultMetadataViewModel(GameSearchResultService gameSearchResultService)
    {
        _gameSearchResultService = gameSearchResultService;
        Sources = [];
        Sources.CollectionChanged += OnSourcesCollectionChanged;
        ReviewsLink = string.Empty;
        DownloadLink = string.Empty;
        WalkthroughLink = string.Empty;
    }

    private void OnSourcesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasMultipleSources));
        OnPropertyChanged(nameof(SourceMenuItems));
    }

    private readonly GameSearchResultService _gameSearchResultService;

    [ObservableProperty]
    public partial string Author { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AuthorFullName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial GameDifficulty Difficulty { get; set; }

    [ObservableProperty]
    public partial GameLength Length { get; set; }

    [ObservableProperty]
    public partial string Setting { get; set; } = string.Empty;

    [ObservableProperty]
    public partial GameEngine Engine { get; set; }

    [ObservableProperty]
    public partial string DetailsLink { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BaseUrl { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TitlePic { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SourceSiteDisplayName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial InstallProgressViewModel? InstallProgress { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasReviews))]
    public partial string ReviewsLink { get; set; }

    public bool HasReviews => ReviewsLink.IsNotNullOrWhiteSpace();
    [ObservableProperty]
    public partial string DownloadLink { get; set; }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasWalkthrough))]
    public partial string WalkthroughLink { get; set; }

    public bool HasWalkthrough => WalkthroughLink.IsNotNullOrWhiteSpace();
    [ObservableProperty]
    public partial int? SizeInMb { get; set; }

    [ObservableProperty]
    public partial double? Rating { get; set; }

    public int ReviewCount => Sources.Sum(s => s.ReviewCount);
    [ObservableProperty]
    public partial DateTime? ReleaseDate { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<IGameSearchResultMetadata> Sources { get; set; }

    [ObservableProperty]
    public partial GameWithStatsViewModel? InstalledGame { get; set; }

    [ObservableProperty]
    public partial bool IsNewlyAdded { get; set; }

    [ObservableProperty]
    public partial bool IsRecentlyUpdated { get; set; }

    public bool HasMultipleSources => Sources.Count > 1;

    public IEnumerable<CommandViewModel> SourceMenuItems =>
        Sources.Select(s =>
        {
            var capturedSource = s;
            return new CommandViewModel
            {
                Text = GetDownloadFromSiteLabel(capturedSource.SourceSiteDisplayName),
                Command = new AsyncRelayCommand(
                    () => InstallFrom(capturedSource),
                    () => _gameSearchResultService.CanInstall(this).GetAwaiter().GetResult())
            };
        });

    [RelayCommand(CanExecute = nameof(CanInstall), AllowConcurrentExecutions = false)]
    private async Task Install()
    {
        try
        {
            await _gameSearchResultService.Install(this);
        }
        catch (OperationCanceledException)
        {
            InstallProgress = null;
        }
    }
    
    private bool CanInstall() => _gameSearchResultService.CanInstall(this).GetAwaiter().GetResult();

    [RelayCommand(CanExecute = nameof(CanInstallFrom), AllowConcurrentExecutions = false)]
    private async Task InstallFrom(IGameSearchResultMetadata? source)
    {
        if (source == null) return;
        try
        {
            await _gameSearchResultService.InstallFromSource(this, source);
        }
        catch (OperationCanceledException)
        {
            InstallProgress = null;
        }
    }
    
    private bool CanInstallFrom(IGameSearchResultMetadata? source) => source != null && _gameSearchResultService.CanInstall(this).GetAwaiter().GetResult();

    private string GetDownloadFromSiteLabel(string siteName)
        => _gameSearchResultService.LocalizationManager.GetLocalizedString("DOWNLOAD_FROM_FORMATTABLE", siteName);


    [RelayCommand]
    private async Task CancelInstall() => await _gameSearchResultService.CancelInstall();
}