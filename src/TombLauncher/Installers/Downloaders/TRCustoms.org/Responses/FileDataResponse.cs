using Newtonsoft.Json;

namespace TombLauncher.Installers.Downloaders.TRCustoms.org.Responses;

public class FileDataResponse
{
    public string? Content { get; set; } = null!;
    public string? UploadType { get; set; } = null!;
    public string? Url { get; set; } = null!;
    public int Size { get; set; }
    [JsonProperty("md5sum")]
    public string? Md5Sum { get; set; } = null!;
}