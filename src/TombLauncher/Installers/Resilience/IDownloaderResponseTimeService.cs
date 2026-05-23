using System;
using System.Threading.Tasks;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Installers.Resilience;

public interface IDownloaderResponseTimeService
{
    void RecordResponseTime(string clientName, TimeSpan elapsed);
    TimeSpan? GetAverageResponseTime(string clientName);

    ValueTask SetCircuitBreakerStatus(string clientName, CircuitBreakerState state);

    event Action<string, CircuitBreakerState> OnMeasureUpdated;
}