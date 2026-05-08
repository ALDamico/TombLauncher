using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Ai.Factories;
using TombLauncher.Ai.Services;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Enums;
using TombLauncher.Localization.Extensions;
using TombLauncher.Mappers;
using TombLauncher.Services;
using TombLauncher.ViewModels.Ai;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class AiSettingsViewModel : SettingsSectionViewModelBase
{
    public AiSettingsViewModel(PageViewModel settingsPage, AiBackendFactory backendFactory, AiMapper modelMapper, NotificationService notificationService, KbUpdateService kbUpdateService) : base("AI_FEATURES", settingsPage, PackIconRemixIconKind.BrainAi3Fill)
    {
        _backendFactory = backendFactory;
        _modelMapper = modelMapper;
        _notificationService = notificationService;
        _kbUpdateService = kbUpdateService;
        AvailableBackendTypes = new List<AiBackendType>() { AiBackendType.Ollama, AiBackendType.LmStudio };
        EmbeddingServiceCheckViewModel = new ServiceCheckViewModel()
            { CheckResultMessage = "CHECKING_EMBEDDING_MODEL".GetLocalizedString(), Status = ServiceCheckStatus.Checking };
        ApiServiceCheckViewModel = new ServiceCheckViewModel()
            { CheckResultMessage = "CHECKING_BACKEND_REACHABLE".GetLocalizedString(), Status = ServiceCheckStatus.Checking };
        KbServiceCheckViewModel = new ServiceCheckViewModel()
            { CheckResultMessage = "", Status = ServiceCheckStatus.Unspecified };
    }

    [ObservableProperty] private ObservableCollection<AiModelViewModel>? _availableModels;
    [ObservableProperty] private AiModelViewModel? _selectedModel;
    private string? _savedModelId;
    public string? SavedModelId { set => _savedModelId = value; }
    [ObservableProperty] private bool _isEnabled;
    [ObservableProperty] private List<AiBackendType> _availableBackendTypes;
    [ObservableProperty] private AiBackendType _selectedBackendType;
    [ObservableProperty] private string _endpoint = string.Empty;
    [ObservableProperty] private string? _apiKey;
    [ObservableProperty] private ServiceCheckViewModel _apiServiceCheckViewModel;
    [ObservableProperty] private ServiceCheckViewModel _embeddingServiceCheckViewModel;
    [ObservableProperty] private string _embeddingModelId;

    [ObservableProperty] private ServiceCheckViewModel _kbServiceCheckViewModel;

    private readonly AiBackendFactory _backendFactory;
    private readonly AiMapper _modelMapper;
    private readonly NotificationService _notificationService;
    private readonly KbUpdateService _kbUpdateService;
    private CancellationTokenSource? _checkCts;

    [RelayCommand]
    private async Task UpdateKb()
    {
        KbServiceCheckViewModel.Status = ServiceCheckStatus.Checking;
        KbServiceCheckViewModel.CheckResultMessage = "KB_UPDATING".GetLocalizedString();
        try
        {
            var progress = new Progress<string>(key =>
                Dispatcher.UIThread.Post(() =>
                    KbServiceCheckViewModel.CheckResultMessage = key.GetLocalizedString()));
            await _kbUpdateService.CheckAndUpdateAsync(progress, CancellationToken.None);
            Dispatcher.UIThread.Post(() => KbServiceCheckViewModel.Status = ServiceCheckStatus.Okay);
        }
        catch (Exception ex)
        {
            Dispatcher.UIThread.Post(() =>
            {
                KbServiceCheckViewModel.Status = ServiceCheckStatus.Error;
                KbServiceCheckViewModel.CheckResultMessage = "KB_UPDATE_FAILED".GetLocalizedString(ex.Message);
            });
        }
    }

    partial void OnEndpointChanged(string? oldValue, string newValue) => ScheduleReachabilityCheck();
    partial void OnSelectedBackendTypeChanged(AiBackendType oldValue, AiBackendType newValue) => ScheduleReachabilityCheck();
    partial void OnEmbeddingModelIdChanged(string? oldValue, string newValue) => ScheduleReachabilityCheck();

    private void ScheduleReachabilityCheck()
    {
        _checkCts?.Cancel();
        _checkCts = new CancellationTokenSource();
        _ = CheckReachabilityWithDebounceAsync(_checkCts.Token);
        _ = CheckEmbeddingModelAsync(_checkCts.Token);
    }

    private async Task CheckEmbeddingModelAsync(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(EmbeddingModelId))
        {
            Dispatcher.UIThread.Post(() =>
            {
                EmbeddingServiceCheckViewModel.Status = ServiceCheckStatus.Unspecified;
                EmbeddingServiceCheckViewModel.CheckResultMessage = "";
            });
            return;
        }
        var backend = _backendFactory.Create(SelectedBackendType);
        Dispatcher.UIThread.Post(() =>
        {
            EmbeddingServiceCheckViewModel.Status = ServiceCheckStatus.Checking;
            EmbeddingServiceCheckViewModel.CheckResultMessage = "CHECKING_EMBEDDING_MODEL".GetLocalizedString();
        });
        try
        {
            var result = await backend.IsModelInstalledAsync(Endpoint, ApiKey!, EmbeddingModelId, ct);
            if (ct.IsCancellationRequested)
                return;
            Dispatcher.UIThread.Post(() =>
            {
                EmbeddingServiceCheckViewModel.Status = result ? ServiceCheckStatus.Okay : ServiceCheckStatus.Error;
                EmbeddingServiceCheckViewModel.CheckResultMessage = result
                    ? "EMBEDDING_MODEL_AVAILABLE".GetLocalizedString()
                    : "EMBEDDING_MODEL_NOT_AVAILABLE".GetLocalizedString();
            });
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Dispatcher.UIThread.Post(() =>
            {
                EmbeddingServiceCheckViewModel.Status = ServiceCheckStatus.Error;
                EmbeddingServiceCheckViewModel.CheckResultMessage = "EMBEDDING_MODEL_CHECK_ERROR".GetLocalizedString(ex.Message);
            });
        }
    }

    private async Task CheckReachabilityWithDebounceAsync(CancellationToken ct)
    {
        try { await Task.Delay(500, ct); }
        catch (OperationCanceledException) { return; }

        var backend = _backendFactory.Create(SelectedBackendType);
        Dispatcher.UIThread.Post(() =>
        {
            ApiServiceCheckViewModel.Status = ServiceCheckStatus.Checking;
            ApiServiceCheckViewModel.CheckResultMessage = "CHECKING_BACKEND_REACHABLE".GetLocalizedString();
        });
        try
        {
            var result = await backend.IsReachableAsync(Endpoint, ApiKey ?? "", ct);
            
            if (ct.IsCancellationRequested) return;
            Dispatcher.UIThread.Post(() =>
            {
                ApiServiceCheckViewModel.Status = result.IsReachable ? ServiceCheckStatus.Okay : ServiceCheckStatus.Error;
                ApiServiceCheckViewModel.CheckResultMessage = result.IsReachable
                    ? "BACKEND_IS_REACHABLE".GetLocalizedString()
                    : "BACKEND_DID_NOT_RESPOND".GetLocalizedString(result.Error!);
            });
            if (result.IsReachable)
            {
                await FetchModelsAsync(ct);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Dispatcher.UIThread.Post(() =>
            {
                ApiServiceCheckViewModel.Status = ServiceCheckStatus.Error;
                ApiServiceCheckViewModel.CheckResultMessage = "BACKEND_NOT_REACHABLE".GetLocalizedString(ex.Message);
            });
        }
    }

    private async Task FetchModelsAsync(CancellationToken ct)
    {
        var backend = _backendFactory.Create(SelectedBackendType);
        var models = await backend.GetAvailableModelsAsync(Endpoint, ApiKey!, ct);
        if (ct.IsCancellationRequested) return;
        var viewModels = _modelMapper.ToObservableCollection(models, _notificationService);
        Dispatcher.UIThread.Post(() =>
        {
            AvailableModels = viewModels;
            SelectedModel = viewModels.FirstOrDefault(m => m.Metadata.ModelId == _savedModelId);
        });
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