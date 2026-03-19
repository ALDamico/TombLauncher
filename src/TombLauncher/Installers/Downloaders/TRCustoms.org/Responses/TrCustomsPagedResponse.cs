using System.Collections.Generic;

namespace TombLauncher.Installers.Downloaders.TRCustoms.org.Responses;

public class TrCustomsPagedResponse<T>
{
    public int CurrentPage { get; set; }
    public int LastPage { get; set; }
    public int TotalCount { get; set; }
    public int ItemsOnPage { get; set; }
    public string? Next { get; set; } = null!;
    public string? Previous { get; set; } = null!;
    public List<T> Results { get; set; } = [];
}