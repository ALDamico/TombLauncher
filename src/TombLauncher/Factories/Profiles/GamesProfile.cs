using System;
using AutoMapper;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;
using TombLauncher.Factories.Mapping;
using TombLauncher.Services;
using TombLauncher.Utils;
using TombLauncher.ViewModels;

namespace TombLauncher.Factories.Profiles;

internal class GamesProfile : Profile
{
    public GamesProfile(Func<Type, object> serviceFactory)
    {
  
        CreateMap<GameWithStatsDto, GameWithStatsViewModel>()
            .ConstructUsing((dto, ctx) =>
                new GameWithStatsViewModel((GameWithStatsService)serviceFactory(typeof(GameWithStatsService))))
            .ForMember(vm => vm.AreCommandsVisible, exp => exp.Ignore())
            .AfterMap((_, vm) => vm.AreCommandsVisible = false);
    }
}