using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Navigation;

namespace TombLauncher.ViewModels;

public partial class GameDataGridRowViewModel : ViewModelBase
{
    public GameDataGridRowViewModel(NavigationManager navigationManager)
    {
        PlayCmd = new RelayCommand(Play);
        OpenCmd = new RelayCommand(Open);
        _navigationManager = navigationManager;
    }

    private void Open()
    {
        Console.WriteLine($"Opening details for {_gameMetadata.Title}");
    }

    private void Play()
    {
        Console.WriteLine($"Playing {_gameMetadata.Title}");
    }

    [ObservableProperty] private GameMetadataViewModel _gameMetadata;
    [ObservableProperty] private bool _areCommandsVisible;
    private NavigationManager _navigationManager;
    public ICommand PlayCmd { get; }
    public ICommand OpenCmd { get; }
}