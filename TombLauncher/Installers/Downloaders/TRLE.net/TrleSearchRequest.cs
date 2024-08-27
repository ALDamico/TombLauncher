namespace TombLauncher.Installers.Downloaders.TRLE.net;

public class TrleSearchRequest
{
    public int Atype { get; set; }
    public string Level { get; set; }
    public int? Rating { get; set; }
    public string Author { get; set; }
    public string Class { get; set; }
    public string Type { get; set; }
    public int SortIdx { get; set; }
    public int Sorttype { get; set; }
    public int? Difficulty { get; set; }
    public int? DurationClass { get; set; }
}