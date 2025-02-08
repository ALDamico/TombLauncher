using System;

namespace TombLauncher.Installers.Downloaders.TRCustoms.org.Responses;

public class LevelVersionResponse
{
    public int Id { get; set; }
    public int Version { get; set; }
    public int Size { get; set; }
    public DateTime? Created { get; set; }
    public string Url { get; set; }
}