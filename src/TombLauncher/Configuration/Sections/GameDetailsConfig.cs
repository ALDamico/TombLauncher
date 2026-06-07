using System.Collections.Generic;
using TombLauncher.Contracts;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Configuration.Sections;

public class GameDetailsConfig : IGameDetailsConfig
{
    public bool? AskForConfirmationBeforeWalkthrough { get; set; }
    public List<CheckableItem<string>>? DocumentationPatterns { get; set; }
    public List<CheckableItem<string>>? DocumentationFolderExclusions { get; set; }
    public int? DescriptionFontSize { get; set; }
}
