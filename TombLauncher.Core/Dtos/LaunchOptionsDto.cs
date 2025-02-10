using TombLauncher.Contracts.Enums;

namespace TombLauncher.Core.Dtos;

public class LaunchOptionsDto
{
    public int GameId { get; set; }
    public GameEngine GameEngine { get; set; }
    public FileBackupDto GameExecutable { get; set; }
    public FileBackupDto SetupExecutable { get; set; }
    public FileBackupDto CommunitySetupExecutable { get; set; }
}