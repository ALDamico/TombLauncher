using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;

namespace TombLauncher.Factories.Mapping;

public class GameExecutableResolver : IValueResolver<Game, GameMetadataDto, string>
{
    public List<FileBackup> Resolve(IGameMetadata source, Game destination, List<FileBackup> destMember, ResolutionContext context)
    {
        var fileBackup = new FileBackup()
        {
            FileName = source.ExecutablePath,
            GameId = source.Id,
            FileType = FileType.GameExecutable
        };
        
        destination.FileBackups.Add(fileBackup);
        destMember = destination.FileBackups;
        return destMember;
    }

    public string Resolve(Game source, GameMetadataDto destination, string destMember, ResolutionContext context)
    {
        var gameExecutable = source.FileBackups.FirstOrDefault(s => s.FileType == FileType.GameExecutable);
        destination.ExecutablePath = gameExecutable?.FileName;
        return destination.ExecutablePath;
    }
}