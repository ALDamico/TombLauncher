using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Services;

namespace TombLauncher.ViewModels;

public partial class MultiSourceGameSearchResultMetadataViewModel : ViewModelBase
{
    public MultiSourceGameSearchResultMetadataViewModel(GameSearchResultService gameSearchResultService)
    {
        _gameSearchResultService = gameSearchResultService;
        Sources = new ObservableCollection<IGameSearchResultMetadata>();
        InstallCmd = new AsyncRelayCommand(Install, CanInstall);
        CancelInstallCmd = new AsyncRelayCommand(CancelInstall, CanCancelInstall);
    }

    private GameSearchResultService _gameSearchResultService;
    
    [ObservableProperty] private string _author;
    [ObservableProperty] private string _authorFullName;
    [ObservableProperty] private string _title;
    [ObservableProperty] private GameDifficulty _difficulty;
    [ObservableProperty] private GameLength _length;
    [ObservableProperty] private string _setting;
    [ObservableProperty] private GameEngine _engine;
    [ObservableProperty] private string _detailsLink;
    [ObservableProperty] private string _baseUrl;
    [ObservableProperty] private Bitmap _titlePic;
    [ObservableProperty] private string _sourceSiteDisplayName;
    [ObservableProperty] private InstallProgressViewModel _installProgress;
    private string _reviewsLink;

    public string ReviewsLink
    {
        get => _reviewsLink;
        set
        {
            SetProperty(ref _reviewsLink, value);
            OnPropertyChanged(nameof(HasReviews));
        }
    }

    public bool HasReviews => !string.IsNullOrWhiteSpace(ReviewsLink);
    [ObservableProperty] private string _downloadLink;
    private string _walkthroughLink;

    public string WalkthroughLink
    {
        get => _walkthroughLink;
        set
        {
            SetProperty(ref _walkthroughLink, value);
            OnPropertyChanged(nameof(HasWalkthrough));
        }
    }

    public bool HasWalkthrough => !string.IsNullOrWhiteSpace(WalkthroughLink);
    [ObservableProperty] private int? _sizeInMb;
    [ObservableProperty] private double? _rating;
    public int ReviewCount => Sources.Sum(s => s.ReviewCount);
    [ObservableProperty] private DateTime? _releaseDate;
    [ObservableProperty] private ObservableCollection<IGameSearchResultMetadata> _sources;
    [ObservableProperty] private GameMetadataViewModel _installedGame;
    private bool _isInstalling;

    public bool IsInstalling
    {
        get => _isInstalling;
        set
        {
            _isInstalling = value;
            OnPropertyChanged();
            ((AsyncRelayCommand)CancelInstallCmd).NotifyCanExecuteChanged();
        }
    }
    
    public ICommand InstallCmd { get; }

    private async Task Install()
    {
        Console.WriteLine($"Attempting install {this.GetHashCode()}");
        try
        {
            await _gameSearchResultService.Install(this);
        }
        catch (OperationCanceledException)
        {
            IsInstalling = false;
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

    private bool CanCancelInstall()
    {
        return _gameSearchResultService.CanCancelInstall(this);
    }
}