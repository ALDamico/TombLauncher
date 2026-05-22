using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Installers.Resilience;

public class DownloaderResponseTimeService : IDownloaderResponseTimeService
{
    private readonly ConcurrentDictionary<string, TimeSpan> _recordings = new();
    private readonly ConcurrentDictionary<string, CircuitBreakerState> _circuitBreakerStates = new();
    public void RecordResponseTime(string clientName, TimeSpan elapsed)
    {
        _recordings[clientName] = elapsed;
        OnMeasureUpdated?.Invoke(clientName, _circuitBreakerStates.GetValueOrDefault(clientName, CircuitBreakerState.Closed));
    }

    public TimeSpan? GetLastResponseTime(string clientName)
    {
        if (_recordings.TryGetValue(clientName, out var responseTime))
            return responseTime;

        return null;
    }

    public ValueTask SetCircuitBreakerStatus(string clientName, CircuitBreakerState state)
    {
        _circuitBreakerStates[clientName] = state;
        OnMeasureUpdated?.Invoke(clientName, state);
        return ValueTask.CompletedTask;
    }

    public event Action<string, CircuitBreakerState>? OnMeasureUpdated;
}