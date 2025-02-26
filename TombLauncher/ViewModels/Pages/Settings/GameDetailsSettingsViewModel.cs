using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class GameDetailsSettingsViewModel : SettingsSectionViewModelBase
{
    public GameDetailsSettingsViewModel(PageViewModel settingsPage) : base("GAME DETAILS", settingsPage)
    {
    }

    [ObservableProperty] private bool _askForConfirmationBeforeWalkthrough;
    [ObservableProperty] private bool _useInternalViewerIfAvailable;
    [ObservableProperty] private EditablePatternListBoxViewModel _documentationPatterns;

    public List<string> GetEnabledPatterns()
    {
        return DocumentationPatterns.TargetCollection
            .Where(p => p.IsChecked)
            .Select(p => p.Value)
            .ToList();
    }
}