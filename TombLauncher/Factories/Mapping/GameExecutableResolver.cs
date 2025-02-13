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
    public string Resolve(Game source, GameMetadataDto destination, string destMember, ResolutionContext context)
    {
        var gameExecutable = source.FileBackups.FirstOrDefault(s => s.FileType == FileType.GameExecutable);
        destination.ExecutablePath = gameExecutable?.FileName;
        return destination.ExecutablePath;
    }
}

public class SetupExecutableResolver : IValueResolver<Game, GameMetadataDto, string>
{
    public string Resolve(Game source, GameMetadataDto destination, string destMember, ResolutionContext context)
    {
        var setupExecutable = source.FileBackups.FirstOrDefault(s => s.FileType == FileType.SetupExecutable);
        if (setupExecutable == null)
            return null;
        destination.SetupExecutable = setupExecutable.FileName;
        destination.SetupExecutableArgs = setupExecutable.Arguments;
        return destination.SetupExecutable;
    }
}

public class CommunitySetupExecutableResolver : IValueResolver<Game, GameMetadataDto, string>
{
    public string Resolve(Game source, GameMetadataDto destination, string destMember, ResolutionContext context)
    {
        var communitySetupExecutable =
            source.FileBackups.FirstOrDefault(s => s.FileType == FileType.CommunitySetupExecutable);
        if (communitySetupExecutable == null)
            return null;
        destination.CommunitySetupExecutable = communitySetupExecutable.FileName;
        return destination.CommunitySetupExecutable;
    }
}

public class GameFileBackupsResolver : IValueResolver<GameMetadataDto, Game, List<FileBackup>>
{
    public List<FileBackup> Resolve(GameMetadataDto source, Game destination, List<FileBackup> destMember, ResolutionContext context)
    {
        var gameExecutable = destination.FileBackups.FirstOrDefault(b => b.FileType == FileType.GameExecutable);
        if (gameExecutable == null)
        {
            gameExecutable = new FileBackup()
            {
                GameId = source.Id,
                FileName = source.ExecutablePath,
                FileType = FileType.GameExecutable
            };
            destination.FileBackups.Add(gameExecutable);
        }
        else
        {
            gameExecutable.FileName = source.ExecutablePath;
            gameExecutable.FileType = FileType.GameExecutable;
        }

        
        if (source.SetupExecutable != null)
        {
            var setupExecutable = destination.FileBackups.FirstOrDefault(b => b.FileType == FileType.SetupExecutable);
            if (setupExecutable == null)
            {
                setupExecutable = new FileBackup()
                {
                    GameId = source.Id,
                    FileName = source.SetupExecutable,
                    Arguments = source.SetupExecutableArgs,
                    FileType = FileType.SetupExecutable
                };
                destination.FileBackups.Add(setupExecutable);
            }
            else
            {
                setupExecutable.FileName = source.SetupExecutable;
                setupExecutable.Arguments = source.SetupExecutableArgs;
            }
        }

        if (source.CommunitySetupExecutable != null)
        {
            var communitySetupExecutable =
                destination.FileBackups.FirstOrDefault(b => b.FileType == FileType.CommunitySetupExecutable);
            if (communitySetupExecutable == null)
            {
                communitySetupExecutable = new FileBackup()
                {
                    GameId = source.Id,
                    FileName = source.CommunitySetupExecutable,
                    FileType = FileType.CommunitySetupExecutable
                };
                destination.FileBackups.Add(communitySetupExecutable);
            }
            else
            {
                communitySetupExecutable.FileName = source.CommunitySetupExecutable;
            }
        }

        return destination.FileBackups;
    }
}