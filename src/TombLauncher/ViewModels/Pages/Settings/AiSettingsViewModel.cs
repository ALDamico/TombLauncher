using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Ai.Factories;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Enums;
using TombLauncher.Localization.Extensions;
using TombLauncher.ViewModels.Ai;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class AiSettingsViewModel : SettingsSectionViewModelBase
{
    public AiSettingsViewModel(PageViewModel settingsPage, AiBackendFactory backendFactory) : base("AI_FEATURES", settingsPage, PackIconRemixIconKind.BrainAi3Fill)
    {
        _backendFactory = backendFactory;
        AvailableBackendTypes = new List<AiBackendType>() { AiBackendType.Ollama, AiBackendType.LmStudio };
    }

    [ObservableProperty] private ObservableCollection<AiModelViewModel>? _availableModels;
    public AiModelViewModel? SelectedModel => AvailableModels?.FirstOrDefault(m => m.IsSelected);
    [ObservableProperty] private bool _isEnabled;
    [ObservableProperty] private List<AiBackendType> _availableBackendTypes;
    [ObservableProperty] private AiBackendType _selectedBackendType;
    [ObservableProperty] private string _endpoint = string.Empty;
    [ObservableProperty] private string? _apiKey;
    [ObservableProperty] private ServiceCheckStatus _aiBackendCheckStatus;
    [ObservableProperty] private string _checkResultMessage;

    private readonly AiBackendFactory _backendFactory;
    private CancellationTokenSource? _checkCts;

    partial void OnEndpointChanged(string? oldValue, string newValue) => ScheduleReachabilityCheck();
    partial void OnSelectedBackendTypeChanged(AiBackendType oldValue, AiBackendType newValue) => ScheduleReachabilityCheck();

    private void ScheduleReachabilityCheck()
    {
        _checkCts?.Cancel();
        _checkCts = new CancellationTokenSource();
        _ = CheckReachabilityWithDebounceAsync(_checkCts.Token);
    }

    private async Task CheckReachabilityWithDebounceAsync(CancellationToken ct)
    {
        try { await Task.Delay(500, ct); }
        catch (OperationCanceledException) { return; }

        var backend = _backendFactory.Create(SelectedBackendType);
        Dispatcher.UIThread.Post(() =>
        {
            AiBackendCheckStatus = ServiceCheckStatus.Checking;
            CheckResultMessage = "CHECKING_BACKEND_REACHABLE".GetLocalizedString();
        });
        try
        {
            var result = await backend.IsReachableAsync(Endpoint, ApiKey ?? "", ct);
            if (ct.IsCancellationRequested) return;
            Dispatcher.UIThread.Post(() =>
            {
                AiBackendCheckStatus = result.IsReachable ? ServiceCheckStatus.Okay : ServiceCheckStatus.Error;
                CheckResultMessage = result.IsReachable
                    ? "BACKEND_IS_REACHABLE".GetLocalizedString()
                    : "BACKEND_DID_NOT_RESPOND".GetLocalizedString(result.Error!);
            });
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Dispatcher.UIThread.Post(() =>
            {
                AiBackendCheckStatus = ServiceCheckStatus.Error;
                CheckResultMessage = "BACKEND_NOT_REACHABLE".GetLocalizedString(ex.Message);
            });
        }
    }
    
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
        userConfig.Ai.Endpoint = Endpoint;
        userConfig.Ai.ApiKey = ApiKey;
        userConfig.Ai.BackendType = SelectedBackendType;
        base.ApplyTo(userConfig);
    }
}