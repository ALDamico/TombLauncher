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
        Watermark = "Insert a new pattern here...".GetLocalizedString();
        Header = "Use the following patterns to recognize documentation files".GetLocalizedString();
    }
    public override ValidationResult Validate(string newValue)
    {
        if (newValue == null)
            return new ValidationResult("Specify a pattern".GetLocalizedString());
        if (TargetCollection.Any(p => p.Value == newValue))
        {
            return new ValidationResult("This pattern has already been added".GetLocalizedString());
        }

        var invalidPatterns = new List<string>() { @"^\*+$", @"^\*+\.\*+$" };
        foreach (var invalidPattern in invalidPatterns)
        {
            if (Regex.IsMatch(newValue, invalidPattern))
                return new ValidationResult("The pattern PATTERN is not allowed".GetLocalizedString(newValue));
        }

        return ValidationResult.Success;
    }
}