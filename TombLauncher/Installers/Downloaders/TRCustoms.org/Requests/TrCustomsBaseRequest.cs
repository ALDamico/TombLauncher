namespace TombLauncher.Installers.Downloaders.TRCustoms.org.Requests;

public class TrCustomsBaseRequest
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string Search { get; set; }
    public string Sort { get; set; }
}