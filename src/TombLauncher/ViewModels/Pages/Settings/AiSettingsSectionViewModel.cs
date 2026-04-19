using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Core.Dtos;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class AiSettingsSectionViewModel : SettingsSectionViewModelBase
{
    public AiSettingsSectionViewModel(PageViewModel settingsPage) : base("AI_FEATURES", settingsPage, PackIconRemixIconKind.BrainAi3Fill)
    {
    }

    [ObservableProperty] private ObservableCollection<AiModelMetadata>? _availableModels;
    [ObservableProperty] private AiModelMetadata? _selectedModel;
    [ObservableProperty] [Range(0, 4)] private int _gpuOffloadLevel;
    [ObservableProperty] private bool _isEnabled;
}