using AutoMapper;
using TombLauncher.Contracts.Localization.Dtos;
using TombLauncher.Core.Dtos;
using TombLauncher.ViewModels.Pages.Settings;

namespace TombLauncher.Factories.Profiles;

internal class SettingsProfile : Profile
{
    public SettingsProfile()
    {
        CreateMap<AvailableLanguageDto, ApplicationLanguageViewModel>()
            .ForMember(dto => dto.CultureInfo, opt => opt.MapFrom(culture => culture.Culture))
            .ReverseMap();
        CreateMap<DownloaderConfiguration, DownloaderViewModel>().ReverseMap();
    }
}