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