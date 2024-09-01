using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Models;

namespace TombLauncher.ViewModels;

public partial class MultiSourceGameSearchResultMetadataViewModel : ViewModelBase, IGameSearchResultMetadata
{
    public MultiSourceGameSearchResultMetadataViewModel()
    {
        Sources = new ObservableCollection<GameSearchResultMetadataViewModel>();
    }
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
    [ObservableProperty] private DateTime _releaseDate;
    [ObservableProperty] private ObservableCollection<GameSearchResultMetadataViewModel> _sources;
    [ObservableProperty] private GameMetadataViewModel _installedGame;
    [ObservableProperty] private double _totalBytes;
    [ObservableProperty] private double _currentBytes;
    [ObservableProperty] private double _downloadSpeed;

}