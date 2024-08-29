﻿using System;

namespace TombLauncher.Progress;

public class DownloadProgressInfo
{
    public DateTime StartDate { get; set; }
    public long TotalBytes { get; set; }
    public long BytesDownloaded { get; set; }

    public double DownloadSpeed
    {
        get
        {
            if ((DateTime.Now - StartDate).TotalSeconds == 0)
            {
                return 0;
            }

            return BytesDownloaded / (DateTime.Now - StartDate).TotalSeconds;
        }
    }
}