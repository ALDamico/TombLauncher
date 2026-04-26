using AutoMapper;
using TombLauncher.Contracts.Localization.Dtos;
using TombLauncher.Core.Dtos;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages.Settings;

namespace TombLauncher.Factories.Profiles;

internal class SettingsProfile : Profile
{
    public SettingsProfile()
    {
        CreateMap<DownloaderConfiguration, DownloaderViewModel>().ReverseMap();
        CreateMap<UnzipBackendDto, UnzipBackendViewModel>().ReverseMap();
    }
}