using System.Collections.Generic;

namespace TombLauncher.Installers.Downloaders.TRCustoms.org;

public class TrCustomsPagedResponse<T>
{
    public int CurrentPage { get; set; }
    public int LastPage { get; set; }
    public int TotalCount { get; set; }
    public int ItemsOnPage { get; set; }
    public string Next { get; set; }
    public string Previous { get; set; }
    public List<T> Results { get; set; }
}