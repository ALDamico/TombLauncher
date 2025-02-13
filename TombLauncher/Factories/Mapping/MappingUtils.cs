using AutoMapper;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.ViewModels.Dialogs;

namespace TombLauncher.Factories.Mapping;

internal static class MappingUtils
{
    internal static FileBackupDto MapGameExecutable(LaunchOptionsDialogViewModel launchOptionsDialogViewModel,
        LaunchOptionsDto launchOptionsDto, FileBackupDto fileBackupDto, ResolutionContext resolutionContext)
    {
        if (launchOptionsDialogViewModel.GameExecutable != null)
        {
            return new FileBackupDto()
            {
                GameId = launchOptionsDialogViewModel.TargetGame.Id,
                FileName = launchOptionsDialogViewModel.GameExecutable, 
                FileType = FileType.GameExecutable
            };
        }

        return fileBackupDto;
    }

    internal static FileBackupDto MapSetupExecutable(LaunchOptionsDialogViewModel launchOptionsDialogViewModel,
        LaunchOptionsDto launchOptionsDto, FileBackupDto fileBackupDto, ResolutionContext resolutionContext)
    {
        if (launchOptionsDialogViewModel.SupportsSetup)
        {
            return new FileBackupDto()
            {
                GameId = launchOptionsDialogViewModel.TargetGame.Id,
                FileName = launchOptionsDialogViewModel.SetupExecutable,
                Arguments = launchOptionsDialogViewModel.SetupArgs,
                FileType = FileType.SetupExecutable
            };
        }

        return null;
    }

    internal static FileBackupDto MapCommunitySetupExecutable(LaunchOptionsDialogViewModel launchOptionsDialogViewModel,
        LaunchOptionsDto launchOptionsDto, FileBackupDto fileBackupDto, ResolutionContext resolutionContext)
    {
        if (launchOptionsDialogViewModel.SupportsCustomSetup)
        {
            return new FileBackupDto()
            {
                GameId = launchOptionsDialogViewModel.TargetGame.Id,
                FileName = launchOptionsDialogViewModel.CustomSetupExecutable,
                FileType = FileType.CommunitySetupExecutable
            };
        }

        return null;
    }
}