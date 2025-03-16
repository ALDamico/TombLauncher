using System.ComponentModel;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class GameDetailsSettingsViewModel : SettingsSectionViewModelBase
{
    public GameDetailsSettingsViewModel(PageViewModel settingsPage) : base("GAME DETAILS", settingsPage)
    {
    }

    public bool IsWinePathOptionVisible => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    [ObservableProperty] private bool _askForConfirmationBeforeWalkthrough;
    [ObservableProperty] private string _winePath;
    private EditablePatternListBoxViewModel _documentationPatterns;

    public EditablePatternListBoxViewModel DocumentationPatterns
    {
        get => _documentationPatterns;
        set
        {
            if (_documentationPatterns != null)
            {
                _documentationPatterns.PropertyChanged -= OnChildPropertyChanged;
            }
            _documentationPatterns = value;
            if (value != null)
            {
                _documentationPatterns.PropertyChanged += OnChildPropertyChanged;
            }
            OnPropertyChanged();
        }
    }
    
    private EditableFolderExclusionsListBoxViewModel _folderExclusions;

    public EditableFolderExclusionsListBoxViewModel FolderExclusions
    {
        get => _folderExclusions;
        set
        {
            if (_folderExclusions != null)
            {
                _folderExclusions.PropertyChanged -= OnChildPropertyChanged;
            }
            _folderExclusions = value;
            if (value != null)
            {
                _folderExclusions.PropertyChanged += OnChildPropertyChanged;
            }
            OnPropertyChanged();
        }
    }

    private void OnChildPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        string propertyName = "";
        if (sender == DocumentationPatterns)
            propertyName = nameof(DocumentationPatterns);
        if (sender == FolderExclusions)
            propertyName = nameof(FolderExclusions);
        OnPropertyChanged(propertyName);
    }
}