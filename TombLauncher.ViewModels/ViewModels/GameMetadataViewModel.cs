using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.ViewModels;

public partial class GameMetadataViewModel : ViewModelBase
{
    public int Id { get; set; }
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _author;
    [ObservableProperty] private DateTime? _releaseDate;
    [ObservableProperty] private DateTime? _installDate;
    [ObservableProperty] private string _gameEngine;
    [ObservableProperty] private string _setting;
    [ObservableProperty] private string _length;
    [ObservableProperty] private string _difficulty;
    [ObservableProperty] private string _installDirectory;
    [ObservableProperty] private string _executablePath;
}