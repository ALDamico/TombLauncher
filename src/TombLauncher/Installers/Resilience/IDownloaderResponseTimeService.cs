using System;

namespace TombLauncher.Installers.Resilience;

public interface IDownloaderResponseTimeService
{
    void RecordResponseTime(string clientName, TimeSpan elapsed);
    TimeSpan? GetLastResponseTime(string clientName);
}