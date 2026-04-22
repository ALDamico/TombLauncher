using System;
using AutoMapper;
using TombLauncher.Ai.Services;
using TombLauncher.Core.Dtos;
using TombLauncher.Services;
using TombLauncher.ViewModels;
using AiModelViewModel = TombLauncher.ViewModels.Ai.AiModelViewModel;

namespace TombLauncher.Factories.Profiles;

public class AiProfile : Profile
{
    public AiProfile(Func<Type, object> serviceFactory)
    {
        CreateMap<AiModelMetadata, AiModelViewModel>().ConstructUsing((dto, _) => new AiModelViewModel(dto,
            (ModelDownloadService)serviceFactory(typeof(ModelDownloadService)),
                (NotificationService)serviceFactory(typeof(NotificationService))));
        
        CreateMap<AiModelViewModel, AiModelMetadata>().ConstructUsing((vm, _) => vm.Metadata);
    }
}