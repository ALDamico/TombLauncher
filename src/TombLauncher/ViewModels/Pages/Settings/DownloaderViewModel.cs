using System;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Contracts.Enums;
using TombLauncher.Installers.Resilience;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class DownloaderViewModel : ObservableObject, IDisposable
{
    private readonly IDownloaderResponseTimeService _responseTimeService;

    private void UpdateDownloaderPerfStatus(string clientName, CircuitBreakerState state)
    {
        if (clientName != ShortClassName)
            return;

        Dispatcher.UIThread.Post(() =>
        {
            if (state == CircuitBreakerState.Open)
            {
                HealthStatus.Status = ServiceCheckStatus.Error;
                HealthStatus.CheckResultMessage = "CIRCUIT_BREAKER_OPEN".GetLocalizedString(DisplayName);
                return;
            }

            if (state == CircuitBreakerState.HalfOpen)
            {
                HealthStatus.Status = ServiceCheckStatus.Warning;
                HealthStatus.CheckResultMessage = "CIRCUIT_BREAKER_HALF_OPEN".GetLocalizedString(DisplayName);
                return;
            }

            var lastResponseTime = _responseTimeService.GetLastResponseTime(ShortClassName);
            if (lastResponseTime == null)
            {
                HealthStatus.Status = ServiceCheckStatus.Unspecified;
                HealthStatus.CheckResultMessage = "NOT_ENOUGH_DATA_FOR_DOWNLOADER".GetLocalizedString(DisplayName);
            }
            else if (lastResponseTime.Value.TotalMilliseconds < 500)
            {
                HealthStatus.Status = ServiceCheckStatus.Okay;
                HealthStatus.CheckResultMessage =
                    "DOWNLOADER_RESPONDED_IN_MS".GetLocalizedString(DisplayName,
                        lastResponseTime.Value.TotalMilliseconds);
            }
            else
            {
                HealthStatus.Status = ServiceCheckStatus.Warning;
                HealthStatus.CheckResultMessage =
                    "DOWNLOADER_PERFORMANCE_DEGRADED".GetLocalizedString(DisplayName,
                        lastResponseTime.Value.TotalMilliseconds);
            }
        });
    }

    public DownloaderViewModel(IDownloaderResponseTimeService responseTimeService)
    {
        HealthStatus = new();
        _responseTimeService = responseTimeService;
        _responseTimeService.OnMeasureUpdated += UpdateDownloaderPerfStatus;
    }

    public void Dispose()
    {
        _responseTimeService.OnMeasureUpdated -= UpdateDownloaderPerfStatus;
    }
    [ObservableProperty]
    public partial string BaseUrl { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DisplayName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsChecked { get; set; }

    [ObservableProperty]
    public partial int Priority { get; set; }

    [ObservableProperty]
    public partial string ClassName { get; set; } = string.Empty;

    public string? ShortClassName => ClassName.Split('.').LastOrDefault();

    [ObservableProperty]
    public partial string SupportedFeatures { get; set; } = string.Empty;
    
    [ObservableProperty] public partial ServiceCheckViewModel HealthStatus { get; set; }
}