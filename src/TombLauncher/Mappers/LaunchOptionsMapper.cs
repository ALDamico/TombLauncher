using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Mappers;

public class LaunchOptionsMapper
{
    public LaunchOptionsDto ToDto(LaunchOptionsViewModel vm)
    {
        FileBackupDto? setupExecutable = null;
        if (vm.SupportsSetup)
        {
            setupExecutable = new FileBackupDto()
            {
                GameId = vm.GameId,
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
                GameId = vm.GameId,
                FileName = vm.CustomSetupExecutable ?? string.Empty,
                FileType = FileType.CommunitySetupExecutable
            };
        }

        FileBackupDto gameExecutable = new FileBackupDto()
        {
            GameId = vm.GameId,
            FileName = vm.GameExecutable!,
            FileType = FileType.GameExecutable
        };

        return new LaunchOptionsDto()
        {
            GameExecutable = gameExecutable,
            GameId = vm.GameId,
            SetupExecutable = setupExecutable,
            CommunitySetupExecutable = communitySetupExecutable,
            GameEngine = vm.SelectedEngine,
            CompatibilityPrefixPath = vm.CompatibilityPrefixPath,
            CompatibilityTool = vm.CompatibilityTool,
            CompatibilityToolPath = vm.CompatibilityToolPath,
            ExtraEnvVars = vm.ExtraEnvVars
        };
    }
}