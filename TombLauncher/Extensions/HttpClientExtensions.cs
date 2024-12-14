using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TombLauncher.Core.Progress;

namespace TombLauncher.Extensions;

public static class HttpClientExtensions
{
    public static async Task DownloadAsync(this HttpClient client, string requestUri, Stream destination, IProgress<DownloadProgressInfo> progress = null, CancellationToken cancellationToken = default) {
        
        // Get the http headers first to examine the content length
        using (var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken)) {
            var contentLength = response.Content.Headers.ContentLength;
            var startDate = DateTime.Now;

            using (var download = await response.Content.ReadAsStreamAsync(cancellationToken)) {

                // Ignore progress reporting when no progress reporter was 
                // passed or when the content length is unknown
                if (progress == null || !contentLength.HasValue) {
                    await download.CopyToAsync(destination, cancellationToken);
                    return;
                }

                // Convert absolute progress (bytes downloaded) into relative progress (0% - 100%)
                var relativeProgress = new Progress<long>(totalBytes => progress.Report(new DownloadProgressInfo(){BytesDownloaded = (long)((double)totalBytes), StartDate = startDate, TotalBytes = contentLength.Value}));
                // Use extension method to report progress while downloading
                await download.CopyToAsync(destination, 81920, relativeProgress, cancellationToken);
                progress.Report(new DownloadProgressInfo(){BytesDownloaded = contentLength.GetValueOrDefault(), StartDate = startDate, TotalBytes = contentLength.Value});
            }
        }
    }
}