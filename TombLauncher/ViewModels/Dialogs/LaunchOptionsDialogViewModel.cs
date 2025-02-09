using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Installers;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels.Dialogs;

public class LaunchOptionsDialogViewModel : DialogViewModel
{
    public LaunchOptionsDialogViewModel()
    {
        SelectedEngine = GameEngine.Unknown;
        AvailableEngines = EnumUtils.GetEnumViewModels<GameEngine>().ToObservableCollection();
        AutoDetectCmd = new RelayCommand(AutoDetect);
    }
    private GameMetadataViewModel _targetGame;

    public GameMetadataViewModel TargetGame
    {
        get => _targetGame;
        set
        {
            _targetGame = value;
            RaiseAndSetIfChanged(ref _targetGame, value);
            SelectedEngine = GetSelectedEngine(value.GameEngine);
            AvailableExecutables = Directory.GetFiles(value.InstallDirectory, "*.exe", SearchOption.AllDirectories)
                .Select(p => Path.GetRelativePath(value.InstallDirectory, p))
                .ToObservableCollection();
            GameExecutable = AvailableExecutables.FirstOrDefault(exe => exe == value.ExecutablePath);
        }
    }

    private ObservableCollection<string> _availableExecutables;

    public ObservableCollection<string> AvailableExecutables
    {
        get => _availableExecutables;
        set
        {
            _availableExecutables = value;
            RaiseAndSetIfChanged(ref _availableExecutables, value);
        }
    }

    private string _gameExecutable;

    public string GameExecutable
    {
        get => _gameExecutable;
        set
        {
            _gameExecutable = value;
            RaiseAndSetIfChanged(ref _gameExecutable, value);
        }
    }

    private ObservableCollection<EnumViewModel<GameEngine>> _availableEngines;

    public ObservableCollection<EnumViewModel<GameEngine>> AvailableEngines
    {
        get => _availableEngines;
        set
        {
            _availableEngines = value;
            RaiseAndSetIfChanged(ref _availableEngines, value);
        }
    }

    private GameEngine _selectedEngine;

    public GameEngine SelectedEngine
    {
        get => _selectedEngine;
        set => RaiseAndSetIfChanged(ref _selectedEngine, value);
    }
    
    public ICommand AutoDetectCmd { get; }

    private void AutoDetect()
    {
        var engineDetector = Ioc.Default.GetRequiredService<TombRaiderEngineDetector>();
        var detectionResult = engineDetector.Detect(TargetGame.InstallDirectory);
        SelectedEngine = GetSelectedEngine(detectionResult.GameEngine);
        GameExecutable = AvailableExecutables.FirstOrDefault(e => e == detectionResult.ExecutablePath) ;
        OnPropertyChanged(nameof(AvailableExecutables));
        OnPropertyChanged(nameof(GameExecutable));
    }

    private GameEngine GetSelectedEngine(GameEngine engine)
    {
        var vm = AvailableEngines.FirstOrDefault(e => e.Value == engine);
        return vm?.Value ?? GameEngine.Unknown;
    }
    
}