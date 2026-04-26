using System.Collections.Generic;
using System.Linq;
using TombLauncher.Contracts.Localization.Dtos;
using TombLauncher.ViewModels.Pages.Settings;

namespace TombLauncher.Factories.Mapping;

public class SettingsMapper
{
    public ApplicationLanguageViewModel ToViewModel(AvailableLanguageDto dto)
    {
        return new ApplicationLanguageViewModel()
        {
            CultureInfo = dto.Culture,
            CountryIso2Code = dto.CountryIso2Code,
            DictionaryName = dto.DictionaryName,
            DisplayName = dto.DisplayName
        };
    }

    public AvailableLanguageDto ToDto(ApplicationLanguageViewModel viewModel)
    {
        return new AvailableLanguageDto()
        {
            Culture = viewModel.CultureInfo,
            CountryIso2Code = viewModel.CountryIso2Code,
            DictionaryName = viewModel.DictionaryName,
            DisplayName = viewModel.DisplayName
        };
    }

    public List<ApplicationLanguageViewModel> ToViewModels(IEnumerable<AvailableLanguageDto> dtos) =>
        dtos.Select(ToViewModel).ToList();
}