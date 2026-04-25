using System.ComponentModel.DataAnnotations;
using System.Linq;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Utils;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.ViewModels;

public class EditableFolderExclusionsListBoxViewModel : EditableListBoxViewModel
{
    public EditableFolderExclusionsListBoxViewModel()
    {
        Watermark = "INSERT_A_FOLDER_NAME_TO_IGNORE".GetLocalizedString();
        Header = "IGNORE_THE_FOLLOWING_FOLDERS_WHEN_SEARCHING_FOR_DO".GetLocalizedString();
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
                "THE_SPECIFIED_FOLDER_NAME_CONTAINS_THE_FOLLOWING_I".GetLocalizedString(invalidCharsStr));
        }

        if (newValue.EndsWithAny('.', ' '))
            return new ValidationResult("FOLDER_NAMES_CANNOT_END_WITH_A_DOR_OR_A_SPACE".GetLocalizedString());

        return ValidationResult.Success!;
    }
}