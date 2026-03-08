using System.ComponentModel;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class GameDetailsSettingsViewModel : SettingsSectionViewModelBase
{
    public GameDetailsSettingsViewModel(PageViewModel settingsPage) : base("GAME_DETAILS", settingsPage, PackIconRemixIconKind.GamepadLine)
    {
    }

    public bool IsWinePathOptionVisible => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    [ObservableProperty] private bool _askForConfirmationBeforeWalkthrough;
    [ObservableProperty] private string _winePath = string.Empty;

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
}