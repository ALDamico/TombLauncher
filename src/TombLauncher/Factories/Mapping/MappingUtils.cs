using System;
using AutoMapper;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Factories.Mapping;

internal static class MappingUtils
{
    internal static FileBackupDto MapGameExecutable(LaunchOptionsViewModel vm,
        LaunchOptionsDto launchOptionsDto, FileBackupDto? fileBackupDto, ResolutionContext resolutionContext)
    {
        if (vm.GameExecutable != null)
        {
            return new FileBackupDto()
            {
                GameId = vm.GameId,
                FileName = vm.GameExecutable,
                FileType = FileType.GameExecutable
            };
        }

        return fileBackupDto!;
    }

    internal static FileBackupDto MapSetupExecutable(LaunchOptionsViewModel vm,
        LaunchOptionsDto launchOptionsDto, FileBackupDto? fileBackupDto, ResolutionContext resolutionContext)
    {
        if (vm.SupportsSetup)
        {
            return new FileBackupDto()
            {
                GameId = vm.GameId,
                FileName = vm.SetupExecutable ?? string.Empty,
                Arguments = vm.SetupArgs,
                FileType = FileType.SetupExecutable
            };
        }

        return null!;
    }

    internal static FileBackupDto MapCommunitySetupExecutable(LaunchOptionsViewModel vm,
        LaunchOptionsDto launchOptionsDto, FileBackupDto? fileBackupDto, ResolutionContext resolutionContext)
    {
        if (vm.SupportsCustomSetup)
        {
            return new FileBackupDto()
            {
                GameId = vm.GameId,
                FileName = vm.CustomSetupExecutable ?? string.Empty,
                FileType = FileType.CommunitySetupExecutable
            };
        }

        return null!;
    }
}