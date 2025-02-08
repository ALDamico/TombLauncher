using System.Collections.Generic;

namespace TombLauncher.Installers.Downloaders.TRCustoms.org.Requests;

public class SearchRequest : TrCustomsBaseRequest
{
    public SearchRequest()
    {
        Difficulties = new List<int>();
        Durations = new List<int>();
        Ratings = new List<int>();
        Tags = new List<int>();
        Genres = new List<int>();
        Engines = new List<int>();
    }
    public string Sort { get; set; }
    public List<int> Tags { get; set; }
    public List<int> Genres { get; set; }
    public List<int> Difficulties { get; set; }
    public List<int> Durations { get; set; }
    public List<int> Ratings { get; set; }
    public List<int> Engines { get; set; }
}