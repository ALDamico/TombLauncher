using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using TombLauncher.Database.UnitOfWork;
using TombLauncher.Models;

namespace TombLauncher.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(GamesUnitOfWork gamesUoW)
    {
        TogglePaneCmd = new RelayCommand(TogglePane);
        MenuItems = new ObservableCollection<MainMenuItemViewModel>()
        {
            new MainMenuItemViewModel()
            {
                ToolTip = "Welcome",
                Icon = MaterialIconKind.HomeOutline,
                Text = "Welcome"
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "My mods",
                Icon = MaterialIconKind.Games,
                Text = "My mods"
            }
        };
        Title = "Tomb Launcher";
        var tombRaider = new Game()
        {
            Title = "Tomb Raider",
            Author = "Core Design",
            ReleaseDate = new DateTime(1996, 11, 29),
            GameEngine = GameEngine.TombRaider1
        };
        gamesUoW.Games.Insert(tombRaider);
        var games = gamesUoW.Games.GetAll().ToList();
    }

    private bool _isPaneOpen;

    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set => SetProperty(ref _isPaneOpen, value);
    }
    
    public ICommand TogglePaneCmd { get; }

    private void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
    }
    
    public ObservableCollection<MainMenuItemViewModel> MenuItems { get; }
}