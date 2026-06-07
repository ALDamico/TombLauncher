using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TombLauncher.Core.Collections;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Installers.Resilience;

public class DownloaderResponseTimeService : IDownloaderResponseTimeService
{
    private const int ElementsToSave = 10;
    private readonly ConcurrentDictionary<string, CircularBuffer<TimeSpan>> _recordings = new();
    private readonly ConcurrentDictionary<string, CircuitBreakerState> _circuitBreakerStates = new();
    public void RecordResponseTime(string clientName, TimeSpan elapsed)
    {
        _recordings.GetOrAdd(clientName, _ => new CircularBuffer<TimeSpan>(ElementsToSave)).Add(elapsed);
        OnMeasureUpdated?.Invoke(clientName, _circuitBreakerStates.GetValueOrDefault(clientName, CircuitBreakerState.Closed));
    }

    public TimeSpan? GetAverageResponseTime(string clientName)
    {
        if (_recordings.TryGetValue(clientName, out var responseTimes))
            return TimeSpan.FromMilliseconds(responseTimes.Items.Sum(i => i.TotalMilliseconds) / responseTimes.Count);

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