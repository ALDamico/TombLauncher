using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Ai;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class AiSettingsViewModel : SettingsSectionViewModelBase
{
    public AiSettingsViewModel(PageViewModel settingsPage) : base("AI_FEATURES", settingsPage, PackIconRemixIconKind.BrainAi3Fill)
    {
    }

    [ObservableProperty] private ObservableCollection<AiModelViewModel>? _availableModels;
    [ObservableProperty] private AiModelViewModel? _selectedModel;
    [ObservableProperty] [Range(0, 4)] private int _gpuOffloadLevel;
    [ObservableProperty] private bool _isEnabled;

    public override void ApplyTo(AppConfiguration userConfig)
    {
        userConfig.Ai.ModelName = SelectedModel?.Metadata.ModelId;
        userConfig.Ai.GpuOffloadPercentage = (double)GpuOffloadLevel / AiConstants.MaxOffloadLevel;
        userConfig.Ai.IsAiEnabled = IsEnabled;
        userConfig.Ai.ModelSizes = AvailableModels?.Where(m => m.FileSizeBytes != null).ToDictionary(m => m.Metadata.ModelId, m => m.FileSizeBytes.GetValueOrDefault());
        base.ApplyTo(userConfig);
    }
}