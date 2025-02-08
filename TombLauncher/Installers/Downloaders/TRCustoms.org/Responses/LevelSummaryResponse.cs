using System;
using System.Collections.Generic;

namespace TombLauncher.Installers.Downloaders.TRCustoms.org.Responses;

public class LevelSummaryResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public LevelEngineResponse Engine { get; set; }
    public LevelDurationResponse Duration { get; set; }
    public List<LevelTagResponse> Tags { get; set; }
    public List<AuthorResponse> Authors { get; set; }
    public AuthorResponse Uploader { get; set; }
    public DateTime? Created { get; set; }
    public FileResponse Cover { get; set; }
    public List<FileResponse> Screenshots { get; set; }
    public List<LevelExternalLinkResponse> ExternalLinks { get; set; }
    public DateTime? LastUpdate { get; set; }
    public DateTime? LastUserContentUpdated { get; set; }
    public int? LastFile { get; set; }
    public int DownloadCount { get; set; }
    public int RatingCount { get; set; }
    public int ReviewCount { get; set; }
    public bool IsApproved { get; set; }
    public string RejectionReason { get; set; }
    public RatingClassResponse RatingClass { get; set; }
}