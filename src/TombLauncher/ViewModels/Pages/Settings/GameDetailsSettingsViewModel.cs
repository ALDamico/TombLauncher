using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Configuration;
using TombLauncher.Core.Dtos;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class GameDetailsSettingsViewModel : SettingsSectionViewModelBase
{
    public GameDetailsSettingsViewModel(PageViewModel settingsPage) : base("GAME_DETAILS", settingsPage, PackIconRemixIconKind.GamepadLine)
    {
    }

    public bool IsWinePathOptionVisible => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    [ObservableProperty] private bool _askForConfirmationBeforeWalkthrough;
    [ObservableProperty] private string _winePath = string.Empty;
    [ObservableProperty] private int _descriptionFontSize = 18;

    public IReadOnlyList<int> AvailableFontSizes { get; } = new[] { 12, 14, 16, 18, 20 };

    public EditablePatternListBoxViewModel? DocumentationPatterns
    {
        get;
        set
        {
            if (field != null)
            {
                field.PropertyChanged -= OnChildPropertyChanged;
            }

            field = value;
            if (value != null)
            {
                value.PropertyChanged += OnChildPropertyChanged;
            }

            OnPropertyChanged();
        }
    }

    public EditableFolderExclusionsListBoxViewModel? FolderExclusions
    {
        get;
        set
        {
            if (field != null)
            {
                field.PropertyChanged -= OnChildPropertyChanged;
            }

            field = value;
            if (value != null)
            {
                value.PropertyChanged += OnChildPropertyChanged;
            }

            OnPropertyChanged();
        }
    }

    private void OnChildPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var propertyName = "";
        if (sender == DocumentationPatterns)
            propertyName = nameof(DocumentationPatterns);
        if (sender == FolderExclusions)
            propertyName = nameof(FolderExclusions);
        OnPropertyChanged(propertyName);
    }

    public override void ApplyTo(AppConfiguration userConfig)
    {
        userConfig.GameDetails.AskForConfirmationBeforeWalkthrough = AskForConfirmationBeforeWalkthrough;
        userConfig.GameDetails.WinePath = WinePath;
        userConfig.GameDetails.DescriptionFontSize = DescriptionFontSize;
        userConfig.GameDetails.DocumentationPatterns = DocumentationPatterns?.TargetCollection.ToList() ?? new List<CheckableItem<string>>();
        userConfig.GameDetails.DocumentationFolderExclusions = FolderExclusions?.TargetCollection.ToList() ?? new List<CheckableItem<string>>();
    }
}