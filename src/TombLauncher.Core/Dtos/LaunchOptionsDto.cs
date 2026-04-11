using System.Collections.Generic;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Core.Dtos;

public class LaunchOptionsDto
{
    public int GameId { get; set; }
    public GameEngine GameEngine { get; set; }
    public required FileBackupDto GameExecutable { get; set; }
    public FileBackupDto? SetupExecutable { get; set; }
    public FileBackupDto? CommunitySetupExecutable { get; set; }
    public string? CompatibilityPrefixPath { get; set; }
    public CompatibilityTool CompatibilityTool { get; set; }
    public string? CompatibilityToolPath { get; set; }
    public List<EnvironmentVariableDto> ExtraEnvVars { get; set; } = [];
}