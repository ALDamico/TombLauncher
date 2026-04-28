using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Installers;
using TombLauncher.Utils;
using System.ComponentModel;

namespace TombLauncher.ViewModels.Dialogs;

public partial class LaunchOptionsDialogViewModel : DialogViewModel
{
    private readonly TombRaiderEngineDetector _engineDetector;

    public LaunchOptionsDialogViewModel(TombRaiderEngineDetector engineDetector)
    {
        _engineDetector = engineDetector;
        SelectedEngine = GameEngine.Unknown;
        AvailableEngines = EnumUtils.GetEnumViewModels<GameEngine>().ToObservableCollection();
        AvailableEngines = EnumUtils.GetEnumViewModels<GameEngine>().ToObservableCollection();
    }

    public GameMetadataViewModel TargetGame
    {
        get;
        init
        {
            RaiseAndSetIfChanged(ref field, value);
            SelectedEngine = GetSelectedEngine(value.GameEngine);
            if (!string.IsNullOrWhiteSpace(value.InstallDirectory))
            {
                AvailableExecutables = Directory.GetFiles(value.InstallDirectory, "*.exe", SearchOption.AllDirectories)
                    .Select(p => Path.GetRelativePath(value.InstallDirectory, p))
                    .ToObservableCollection();
            }
            else
            {
                AvailableExecutables = new ObservableCollection<string>();
            }

            GameExecutable = AvailableExecutables.FirstOrDefault(exe => exe == value.ExecutablePath);
            SetupArgs = value.SetupExecutableArgs;
            SetupExecutable = AvailableExecutables.FirstOrDefault(exe => exe == value.SetupExecutable);
            CustomSetupExecutable = AvailableExecutables.FirstOrDefault(exe => exe == value.CommunitySetupExecutable);
            if (SetupExecutable.IsNotNullOrWhiteSpace())
            {
                SupportsSetup = true;
            }

            if (CustomSetupExecutable.IsNotNullOrWhiteSpace())
            {
                SupportsCustomSetup = true;
            }
        }
    } = null!;
