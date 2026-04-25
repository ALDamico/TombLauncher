using System;

namespace TombLauncher.Installers.Downloaders.TRCustoms.org.Responses;

public class LevelGenreResponse
{
    public int Id { get; set; }
    public string? Name { get; set; } = null!;
    public string? Description { get; set; } = null!;
    public int LevelCount { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? LastUpdated { get; set; }
}