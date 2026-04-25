namespace TombLauncher.Installers.Downloaders.TRCustoms.org.Responses;

public class FileResponse
{
    public int Id { get; set; }
    public string? Content { get; set; } = null!;
    public string? UploadType { get; set; } = null!;
    public string? Url { get; set; } = null!;
    public int Size { get; set; }
    public string? Md5Sum { get; set; } = null!;
}