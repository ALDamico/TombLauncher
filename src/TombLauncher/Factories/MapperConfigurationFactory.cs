using System;
using AutoMapper;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;
using Newtonsoft.Json;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Localization.Dtos;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Savegames;
using TombLauncher.Core.Utils;
using TombLauncher.Data.Models;
using TombLauncher.Factories.Mapping;
using TombLauncher.Factories.Profiles;
using TombLauncher.Utils;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Dialogs;
using TombLauncher.ViewModels.Pages.Settings;

namespace TombLauncher.Factories;

public static class MapperConfigurationFactory
{
    public static MapperConfiguration GetMapperConfiguration(Func<Type, object>? serviceFactory = null)
    {
        var mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullDestinationValues = true;

            if (serviceFactory != null)
            {
                // Wrap the factory to fallback to Activator.CreateInstance for types
                // not registered in DI (e.g. AutoMapper value resolvers).
                cfg.ConstructServicesUsing(type => serviceFactory(type) ?? Activator.CreateInstance(type));
            }

            cfg.AddProfile<AppCrashProfile>();
            cfg.AddProfile(new GamesProfile(serviceFactory ?? (_ => null!)));
            cfg.AddProfile<SettingsProfile>();
            cfg.AddProfile(new SearchProfile(serviceFactory ?? (_ => null!)));
            cfg.AddProfile<StatisticsProfile>();
            cfg.AddProfile<SavegamesProfile>();
            cfg.AddProfile<LaunchOptionsProfile>();

            cfg.CreateMap<PlaySession, PlaySessionDto>().ReverseMap();
            cfg.CreateMap<FileBackup, FileBackupDto>().ReverseMap();


            cfg.AddGlobalIgnore("InitCmd");
        });

        return mapperConfiguration;
    }
}