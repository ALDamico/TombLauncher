using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels;

public partial class MultiSourceGameSearchResultMetadataViewModel : ViewModelBase
{
    public MultiSourceGameSearchResultMetadataViewModel()
    {
        _gameSearchResultService = Ioc.Default.GetRequiredService<GameSearchResultService>();
        Sources = new ObservableCollection<IGameSearchResultMetadata>();
        InstallCmd = new AsyncRelayCommand(Install, CanInstall);
        CancelInstallCmd = new AsyncRelayCommand(CancelInstall);
    }

    private readonly GameSearchResultService _gameSearchResultService;
    
    [ObservableProperty] private string _author;
    [ObservableProperty] private string _authorFullName;
    [ObservableProperty] private string _title;
    [ObservableProperty] private GameDifficulty _difficulty;
    [ObservableProperty] private GameLength _length;
    [ObservableProperty] private string _setting;
    [ObservableProperty] private GameEngine _engine;
    [ObservableProperty] private string _detailsLink;
    [ObservableProperty] private string _baseUrl;
    [ObservableProperty] private string _titlePic;
    [ObservableProperty] private string _sourceSiteDisplayName;
    [ObservableProperty] private string _description;
    [ObservableProperty] private InstallProgressViewModel _installProgress;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasReviews))]private string _reviewsLink;
    public bool HasReviews => ReviewsLink.IsNotNullOrWhiteSpace();
    [ObservableProperty] private string _downloadLink;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasWalkthrough))]private string _walkthroughLink;
    public bool HasWalkthrough => WalkthroughLink.IsNotNullOrWhiteSpace();
    [ObservableProperty] private int? _sizeInMb;
    [ObservableProperty] private double? _rating;
    public int ReviewCount => Sources.Sum(s => s.ReviewCount);
    [ObservableProperty] private DateTime? _releaseDate;
    [ObservableProperty] private ObservableCollection<IGameSearchResultMetadata> _sources;
    [ObservableProperty] private GameWithStatsViewModel _installedGame;
    
    public ICommand InstallCmd { get; }

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

    private bool CanInstall()
    {
        return _gameSearchResultService.CanInstall(this);
    }
    public ICommand CancelInstallCmd { get; }

    private async Task CancelInstall()
    {
        await _gameSearchResultService.CancelInstall();
    }
}