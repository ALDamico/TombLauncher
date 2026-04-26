using System;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;
using TombLauncher.Factories.Profiles;

namespace TombLauncher.Factories;

public static class MapperConfigurationFactory
{
    public static MapperConfiguration GetMapperConfiguration(ILoggerFactory loggerFactory, Func<Type, object>? serviceFactory = null)
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

            cfg.AddProfile<StatisticsProfile>();
            cfg.AddProfile<SavegamesProfile>();


            cfg.AddGlobalIgnore("InitCommand");
        }, loggerFactory);

        return mapperConfiguration;
    }
}