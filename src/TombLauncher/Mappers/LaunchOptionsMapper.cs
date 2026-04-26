using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.ViewModels.Dialogs;

namespace TombLauncher.Mappers;

public class LaunchOptionsMapper
{
    public LaunchOptionsDto ToDto(LaunchOptionsDialogViewModel vm)
    {
        FileBackupDto? setupExecutable = null;
        if (vm.SupportsSetup)
        {
            setupExecutable = new FileBackupDto()
            {
                GameId = vm.TargetGame.Id,
                FileName = vm.SetupExecutable ?? string.Empty,
                Arguments = vm.SetupArgs,
                FileType = FileType.SetupExecutable
            };
        }

        FileBackupDto? communitySetupExecutable = null;
        if (vm.SupportsCustomSetup)
        {
            communitySetupExecutable = new FileBackupDto()
            {
                GameId = vm.TargetGame.Id,
                FileName = vm.CustomSetupExecutable ?? string.Empty,
                FileType = FileType.CommunitySetupExecutable
            };
        }

        FileBackupDto gameExecutable = new FileBackupDto()
        {
            GameId = vm.TargetGame.Id,
            FileName = vm.GameExecutable!,
            FileType = FileType.GameExecutable
        };
        

        return new LaunchOptionsDto()
        {
            GameExecutable = gameExecutable,
            GameId = vm.TargetGame.Id,
            SetupExecutable = setupExecutable,
            CommunitySetupExecutable = communitySetupExecutable,
            GameEngine = vm.SelectedEngine
        };
    }
}