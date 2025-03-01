﻿using TombLauncher.Contracts.Enums;

namespace TombLauncher.Data.Models;

public class FileBackup
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public byte[] Data { get; set; }
    public DateTime BackedUpOn { get; set; }
    public FileType FileType { get; set; }
    public int GameId { get; set; }
    public string Md5 { get; set; }
    public SavegameMetadata SavegameMetadata { get; set; }
    public string Arguments { get; set; }
    public Game Game { get; set; }
}