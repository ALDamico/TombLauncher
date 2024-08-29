﻿using System;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Models;

namespace TombLauncher.ViewModels;

public partial class GameSearchResultMetadataViewModel : ViewModelBase
{
    [ObservableProperty] private string _author;
    [ObservableProperty] private string _authorFullName;
    [ObservableProperty] private string _title;
    [ObservableProperty] private GameDifficulty _difficulty;
    [ObservableProperty] private GameLength _length;
    [ObservableProperty] private string _setting;
    [ObservableProperty] private GameEngine _engine;
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
    [ObservableProperty] private int _reviewCount;
    [ObservableProperty] private DateTime _releaseDate;
}