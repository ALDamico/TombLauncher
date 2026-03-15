using System.Collections.Generic;

namespace TombLauncher.Installers.Downloaders.TRLE.net;

public class TrleSearchRequest
{
    public int Atype { get; set; }
    public string? Level { get; set; }
    public int? Rating { get; set; }
    public string? Author { get; set; }
    public string? Class { get; set; }
    public string? Type { get; set; }
    public int SortIdx { get; set; }
    public int Sorttype { get; set; }
    public int? Difficulty { get; set; }
    public int? DurationClass { get; set; }
    public int? Idx { get; set; }

    public IEnumerable<KeyValuePair<string, string>> ToQueryParams()
    {
        yield return new("atype", Atype.ToString());
        yield return new("level", Level ?? string.Empty);
        yield return new("rating", Rating?.ToString() ?? string.Empty);
        yield return new("author", Author ?? string.Empty);
        yield return new("class", Class ?? string.Empty);
        yield return new("type", Type ?? string.Empty);
        yield return new("sortidx", SortIdx.ToString());
        yield return new("sorttype", Sorttype.ToString());
        yield return new("difficulty", Difficulty?.ToString() ?? string.Empty);
        yield return new("durationclass", DurationClass?.ToString() ?? string.Empty);
        yield return new("idx", Idx?.ToString() ?? string.Empty);
    }
}