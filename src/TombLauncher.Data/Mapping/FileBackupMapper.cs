using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Mapping;

public class FileBackupMapper
{
    public FileBackupDto ToDto(FileBackup fileBackup)
    {
        return new FileBackupDto()
        {
            FileName = fileBackup.FileName,
            Id = fileBackup.Id,
            GameId = fileBackup.GameId,
            Arguments = fileBackup.Arguments,
            BackedUpOn = fileBackup.BackedUpOn,
            Data = fileBackup.Data,
            FileType = fileBackup.FileType,
            Md5 = fileBackup.Md5
        };
    }

    public List<FileBackupDto> ToDtos(IEnumerable<FileBackup> fileBackups) => fileBackups.Select(ToDto).ToList();

    public FileBackup ToFileBackup(FileBackupDto dto)
    {
        return new FileBackup()
        {
            Id = dto.Id,
            GameId = dto.GameId,
            FileName = dto.FileName,
            Arguments = dto.Arguments,
            BackedUpOn = dto.BackedUpOn,
            Data = dto.Data,
            FileType = dto.FileType,
            Md5 = dto.Md5,
        };
    }

    public List<FileBackup> ToFileBackups(IEnumerable<FileBackupDto> dtos) => dtos.Select(ToFileBackup).ToList();

    public SavegameBackupDto? ToSavegameBackupDto(FileBackup? fileBackup)
    {
        if (fileBackup == null)
            return null;
        return new SavegameBackupDto()
        {
            FileName = fileBackup.FileName,
            Data = fileBackup.Data ?? [],
            LevelName = fileBackup.SavegameMetadata?.LevelName ?? "",
            Md5 = fileBackup.Md5 ?? "",
            Id = fileBackup.Id,
            GameId = fileBackup.GameId,
            BackedUpOn = fileBackup.BackedUpOn,
            FileType = fileBackup.FileType,
            MetadataId = fileBackup.SavegameMetadata?.Id ?? 0,
            SaveNumber = fileBackup.SavegameMetadata?.SaveNumber,
            SlotNumber = fileBackup.SavegameMetadata?.SlotNumber ?? 0,
            GameEngine = fileBackup.Game?.GameEngine ?? default
        };
    }

    public List<SavegameBackupDto> ToSavegameBackupDtos(IEnumerable<FileBackup?> fileBackups) =>
        fileBackups.Where(b => b != null).Select(b => ToSavegameBackupDto(b)!).ToList();

    public FileBackup ToFileBackup(SavegameBackupDto dto)
    {
        return new FileBackup()
        {
            Id = dto.Id,
            GameId = dto.GameId,
            FileName = dto.FileName,
            BackedUpOn = dto.BackedUpOn,
            Data = dto.Data,
            FileType = dto.FileType,
            Md5 = dto.Md5,
            SavegameMetadata = new SavegameMetadata()
            {
                Id = dto.MetadataId,
                LevelName = dto.LevelName,
                SaveNumber = dto.SaveNumber,
                SlotNumber = dto.SlotNumber
            }
        };
    }

    public List<FileBackup> ToFileBackups(IEnumerable<SavegameBackupDto> dtos) => dtos.Select(ToFileBackup).ToList();
}