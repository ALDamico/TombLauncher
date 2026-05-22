using System;
using System.Collections.Concurrent;

namespace TombLauncher.Installers.Resilience;

public class DownloaderResponseTimeService : IDownloaderResponseTimeService
{
    private readonly ConcurrentDictionary<string, TimeSpan> _recordings = new();
    public void RecordResponseTime(string clientName, TimeSpan elapsed)
    {
        _recordings[clientName] = elapsed;
    }

    public TimeSpan? GetLastResponseTime(string clientName)
    {
        if (_recordings.TryGetValue(clientName, out var responseTime))
            return responseTime;

        return null;
    }
}