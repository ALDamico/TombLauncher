using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Configuration;
using TombLauncher.ViewModels.Ai;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class AiSettingsViewModel : SettingsSectionViewModelBase
{
    public AiSettingsViewModel(PageViewModel settingsPage) : base("AI_FEATURES", settingsPage, PackIconRemixIconKind.BrainAi3Fill)
    {
    }

    [ObservableProperty] private ObservableCollection<AiModelViewModel>? _availableModels;
    public AiModelViewModel? SelectedModel => AvailableModels?.FirstOrDefault(m => m.IsSelected);
    [ObservableProperty] private bool _isEnabled;
    
    partial void OnAvailableModelsChanged(ObservableCollection<AiModelViewModel>? oldValue, ObservableCollection<AiModelViewModel>? newValue)
    {
        if (oldValue != null)
            foreach (var item in oldValue)
                item.PropertyChanged -= OnModelPropertyChanged;

        if (newValue != null)
            foreach (var item in newValue)
                item.PropertyChanged += OnModelPropertyChanged;
    }
    
    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AiModelViewModel.IsSelected))
            OnPropertyChanged(nameof(AvailableModels));
    }

    public override void ApplyTo(AppConfiguration userConfig)
    {
        userConfig.Ai.ModelId = SelectedModel?.Metadata.ModelId;
        userConfig.Ai.IsAiEnabled = IsEnabled;
        base.ApplyTo(userConfig);
    }
}