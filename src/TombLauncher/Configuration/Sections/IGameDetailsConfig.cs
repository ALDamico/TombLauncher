using System.Collections.Generic;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Configuration.Sections;

public interface IGameDetailsConfig
{
    bool? AskForConfirmationBeforeWalkthrough { get; }
    string? WinePath { get; }
    List<CheckableItem<string>>? DocumentationPatterns { get; }
    List<CheckableItem<string>>? DocumentationFolderExclusions { get; }
}
