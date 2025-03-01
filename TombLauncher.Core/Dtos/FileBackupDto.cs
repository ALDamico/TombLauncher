﻿using TombLauncher.Contracts.Enums;

namespace TombLauncher.Core.Dtos;

public class FileBackupDto
{
    public FileBackupDto()
    {
        BackedUpOn = DateTime.Now;
    }
    public int Id { get; set; }
    public string FileName { get; set; }
    public byte[] Data { get; set; }
    public DateTime BackedUpOn { get; set; }
    public FileType FileType { get; set; }
    public int GameId { get; set; }
    public string Md5 { get; set; }
    public string Arguments { get; set; }
}