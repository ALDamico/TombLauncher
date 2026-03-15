using System.Collections.Generic;

namespace TombLauncher.Installers.Downloaders.AspideTR.com;

public class AspideTrSearchRequest
{
    public string? Classes { get; set; }
    public int? LType { get; set; }
    public string? LTitle { get; set; }
    public string? LAuthor { get; set; }

    public IEnumerable<KeyValuePair<string, string>> ToQueryParams()
    {
        yield return new("classes", Classes ?? string.Empty);
        yield return new("ltype", LType?.ToString() ?? string.Empty);
        yield return new("ltitle", LTitle ?? string.Empty);
        yield return new("lauthor", LAuthor ?? string.Empty);
    }
}