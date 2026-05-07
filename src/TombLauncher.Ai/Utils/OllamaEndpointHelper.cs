namespace TombLauncher.Ai.Utils;

internal static class OllamaEndpointHelper
{
    internal static string NormalizeEndpoint(string endpoint)
    {
        endpoint = endpoint.TrimEnd('/');
        if (!endpoint.EndsWith("v1", StringComparison.OrdinalIgnoreCase))
            endpoint += "/v1";
        return endpoint;
    }
}
