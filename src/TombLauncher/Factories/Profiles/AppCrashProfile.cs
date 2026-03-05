using System;
using AutoMapper;
using Newtonsoft.Json;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;

namespace TombLauncher.Factories.Profiles;

internal class AppCrashProfile : Profile
{
    public AppCrashProfile()
    {
        CreateMap<AppCrash, AppCrashDto>()
            .ForMember(dto => dto.ExceptionDto,
                opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ExceptionDto>(s.Exception)));
        CreateMap<Exception, ExceptionDto>();
    }
}