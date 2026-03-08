using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using TombLauncher.Localization.Extensions;
using TombLauncher.ViewModels.Pages.Settings;

namespace TombLauncher.ViewModels;

public class EditablePatternListBoxViewModel : EditableListBoxViewModel
{
    public EditablePatternListBoxViewModel()
    {
        Watermark = "INSERT_A_NEW_PATTERN_HERE".GetLocalizedString();
        Header = "USE_THE_FOLLOWING_PATTERNS_TO_RECOGNIZE_DOCUMENTAT".GetLocalizedString();
    }
    public override ValidationResult Validate(string newValue)
    {
        if (newValue == null)
            return new ValidationResult("SPECIFY_A_PATTERN".GetLocalizedString());
        if (TargetCollection.Any(p => p.Value == newValue))
        {
            return new ValidationResult("THIS_PATTERN_HAS_ALREADY_BEEN_ADDED".GetLocalizedString());
        }

        var invalidPatterns = new List<string>() { @"^\*+$", @"^\*+\.\*+$" };
        foreach (var invalidPattern in invalidPatterns)
        {
            if (Regex.IsMatch(newValue, invalidPattern))
                return new ValidationResult("THE_PATTERN_IS_NOT_ALLOWED".GetLocalizedString(newValue));
        }

        return ValidationResult.Success!;
    }
}