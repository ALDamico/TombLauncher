﻿using System;

namespace TombLauncher.Dto;

public class GameWithStatsDto
{
    public GameMetadataDto GameMetadata { get; set; }
    public DateTime? LastPlayed { get; set; }
    public TimeSpan TotalPlayTime { get; set; }
}