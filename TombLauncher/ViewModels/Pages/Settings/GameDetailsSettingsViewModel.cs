using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
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
        AddPatternCmd = new RelayCommand(AddPattern, CanAddPattern);
        ClearCurrentPatternCmd = new RelayCommand(ClearCurrentPattern, CanClearCurrentPattern);
    }

    [ObservableProperty] private bool _askForConfirmationBeforeWalkthrough;
    [ObservableProperty] private bool _useInternalViewerIfAvailable;
    [ObservableProperty] private ObservableCollection<CheckableItem<string>> _documentationPatterns;
    private string _currentPattern;

    
    [CustomValidation(typeof(GameDetailsSettingsViewModel), nameof(ValidatePattern))]
    public string CurrentPattern
    {
        get => _currentPattern;
        set
        {
            SetProperty(ref _currentPattern, value, true);
            RaiseCanExecuteChanged(ClearCurrentPatternCmd);
            RaiseCanExecuteChanged(AddPatternCmd);
        }
    }
    public IRelayCommand AddPatternCmd { get; }

    private void AddPattern()
    {
        DocumentationPatterns.Add(new CheckableItem<string>(){IsEnabled = true, Value = CurrentPattern});
        CurrentPattern = string.Empty;
    }

    private bool CanAddPattern() => CurrentPattern.IsNotNullOrWhiteSpace();
    
    public IRelayCommand ClearCurrentPatternCmd { get; }

    private void ClearCurrentPattern()
    {
        CurrentPattern = string.Empty;
    }

    private bool CanClearCurrentPattern()
    {
        return CurrentPattern.IsNotNullOrWhiteSpace();
    }

    public List<string> GetEnabledPatterns()
    {
        return DocumentationPatterns.Where(p => p.IsEnabled).Select(p => p.Value).ToList();
    }

    public static ValidationResult ValidatePattern(string newPattern, ValidationContext validationContext)
    {
        var instance = (GameDetailsSettingsViewModel)validationContext.ObjectInstance;
        if (instance.DocumentationPatterns.Any(p => p.Value == newPattern))
        {
            return new ValidationResult("This pattern has already been added".GetLocalizedString());
        }

        var invalidPatterns = new List<string>() { @"^\*+$", @"^\*+\.\*+$" };
        foreach (var invalidPattern in invalidPatterns)
        {
            if (Regex.IsMatch(newPattern, invalidPattern))
                return new ValidationResult("The pattern PATTERN is not allowed".GetLocalizedString(newPattern));
        }
        
        return ValidationResult.Success;
    }
}