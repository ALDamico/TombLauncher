using System;

namespace TombLauncher.Installers.Downloaders.TRCustoms.org.Responses;

public class LevelTagResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int LevelCount { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? LastUpdated { get; set; }
}