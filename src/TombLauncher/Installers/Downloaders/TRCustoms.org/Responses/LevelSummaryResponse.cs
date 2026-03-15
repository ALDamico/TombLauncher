using System;
using System.Collections.Generic;

namespace TombLauncher.Installers.Downloaders.TRCustoms.org.Responses;

public class LevelSummaryResponse
{
    public int Id { get; set; }
    public string? Name { get; set; } = null!;
    public string? Description { get; set; } = null!;
    public LevelEngineResponse? Engine { get; set; } = null;
    public LevelDurationResponse? Duration { get; set; } = null;
    public LevelDifficultyResponse? Difficulty { get; set; } = null;
    public List<LevelTagResponse> Tags { get; set; } = null!;
    public List<AuthorResponse> Authors { get; set; } = null!;
    public AuthorResponse Uploader { get; set; } = null!;
    public DateTime? Created { get; set; }
    public FileResponse? Cover { get; set; }
    public List<FileResponse>? Screenshots { get; set; }
    public List<LevelExternalLinkResponse>? ExternalLinks { get; set; }
    public DateTime? LastUpdate { get; set; }
    public DateTime? LastUserContentUpdated { get; set; }
    public LevelVersionResponse? LastFile { get; set; }
    public int DownloadCount { get; set; }
    public int RatingCount { get; set; }
    public int ReviewCount { get; set; }
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
    public RatingClassResponse? RatingClass { get; set; }
}