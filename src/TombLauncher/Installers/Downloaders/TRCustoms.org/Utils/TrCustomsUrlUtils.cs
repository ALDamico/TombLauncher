using TombLauncher.Core.Extensions;

namespace TombLauncher.Installers.Downloaders.TRCustoms.org.Utils;

public static class TrCustomsUrlUtils
{
    public static string GetReviewsLink(string baseUrl, int id) =>
        $"levels/{id}/reviews".EnsureStartsWith(baseUrl, '/');

    public static string GetDetailsLink(string baseUrl, int id) 
        => $"levels/{id}".EnsureStartsWith(baseUrl, '/');
    
    public static string GetApiDetailsLink(string baseUrl, string id) 
        => $"api/levels/{id}".EnsureStartsWith(baseUrl, '/');

    public static string GetDownloadLink(string baseUrl, string? url) =>
        url?.EnsureStartsWith(baseUrl, '/') ?? string.Empty;
}