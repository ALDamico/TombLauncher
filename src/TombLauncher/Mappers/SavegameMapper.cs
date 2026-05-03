using System;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.ViewModels;

namespace TombLauncher.Mappers;

public class SavegameMapper
{
    public FileBackupDto ToDto(SavegameViewModel vm)
    {
        return new FileBackupDto()
        {
            Data = Array.Empty<byte>(),
            Md5 = "",
            FileName = vm.Filename,
            BackedUpOn = vm.BackedUpOn.GetValueOrDefault(),
            FileType = vm.IsStartOfLevel ? FileType.SavegameStartOfLevel : FileType.Savegame,
            Id = vm.Id
        };
    }
}