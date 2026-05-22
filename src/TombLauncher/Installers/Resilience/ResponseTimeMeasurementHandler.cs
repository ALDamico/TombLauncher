using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TombLauncher.Installers.Resilience;

public class ResponseTimeMeasurementHandler : DelegatingHandler
{
    private readonly IDownloaderResponseTimeService _downloaderResponseTimeService;
    private readonly string _clientName;

    public ResponseTimeMeasurementHandler(string clientName, IDownloaderResponseTimeService downloaderResponseTimeService)
    {
        _clientName = clientName;
        _downloaderResponseTimeService = downloaderResponseTimeService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();
        var response = await base.SendAsync(request, cancellationToken);
        stopWatch.Stop();
        if (response.IsSuccessStatusCode)
            _downloaderResponseTimeService.RecordResponseTime(_clientName, stopWatch.Elapsed);
        return response;
    }
}