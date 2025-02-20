using System;

namespace TombLauncher.Installers.Downloaders.TRCustoms.org.Responses;

public class LevelGenreResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int LevelCount { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? LastUpdated { get; set; }
}