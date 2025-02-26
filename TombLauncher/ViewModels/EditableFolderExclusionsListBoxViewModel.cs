using System.ComponentModel.DataAnnotations;
using System.Linq;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Utils;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.ViewModels;

public class EditableFolderExclusionsListBoxViewModel: EditableListBoxViewModel 
{
    public EditableFolderExclusionsListBoxViewModel()
    {
        Watermark = "Insert a folder name to ignore...".GetLocalizedString();
        Header = "Ignore the following folders when searching for documentation".GetLocalizedString();
    }
    public override ValidationResult Validate(string newValue)
    {
        var invalidChars = PathUtils.GetWindowsInvalidFileNameChars();
        var intersection = newValue.Intersect(invalidChars).Select(c => $"""
                                                                         "{c}"
                                                                         """).ToArray();
        if (intersection.Length > 0)
        {
            var invalidCharsStr = string.Join(", ", intersection);
            return new ValidationResult(
                "The specified folder name contains the following invalid characters: CHARLIST".GetLocalizedString(invalidCharsStr));
        }

        if (newValue.EndsWithAny('.', ' '))
            return new ValidationResult("Folder names cannot end with a dor or a space".GetLocalizedString());
        
        return ValidationResult.Success;
    }
}