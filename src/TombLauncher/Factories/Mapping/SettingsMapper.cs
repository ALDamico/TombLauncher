using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TombLauncher.Contracts.Localization.Dtos;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.ViewModels;
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

    public DownloaderConfiguration ToDownloaderConfiguration(DownloaderViewModel viewModel)
    {
        return new DownloaderConfiguration()
        {
            DisplayName = viewModel.DisplayName,
            BaseUrl = viewModel.BaseUrl,
            ClassName = viewModel.ClassName,
            Priority = viewModel.Priority,
            SupportedFeatures = viewModel.SupportedFeatures,
            CanUserCheck = true,
            IsChecked = viewModel.IsChecked
        };
    }

    public List<DownloaderConfiguration> ToDownloaderConfigurations(IEnumerable<DownloaderViewModel> viewModels) =>
        viewModels.Select(ToDownloaderConfiguration).ToList();

    public DownloaderViewModel ToViewModel(DownloaderConfiguration dto)
    {
        return new DownloaderViewModel()
        {
            DisplayName = dto.DisplayName,
            BaseUrl = dto.BaseUrl,
            ClassName = dto.ClassName ?? "",
            IsChecked = dto.IsChecked,
            Priority = dto.Priority,
            SupportedFeatures = dto.SupportedFeatures ?? ""
        };
    }

    public List<DownloaderViewModel> ToViewModels(IEnumerable<DownloaderConfiguration> dtos) =>
        dtos.Select(ToViewModel).ToList();

    public UnzipBackendViewModel ToViewModel(UnzipBackendDto dto)
    {
        return new UnzipBackendViewModel()
        {
            Command = dto.Command,
            CommandLineArguments = dto.CommandLineArguments,
            Name = dto.Name
        };
    }

    public IEnumerable<UnzipBackendViewModel> ToViewModels(IEnumerable<UnzipBackendDto> dtos) =>
        dtos.Select(ToViewModel);

    public ObservableCollection<UnzipBackendViewModel> ToObservableCollection(IEnumerable<UnzipBackendDto> dtos) =>
        ToViewModels(dtos).ToObservableCollection();
}