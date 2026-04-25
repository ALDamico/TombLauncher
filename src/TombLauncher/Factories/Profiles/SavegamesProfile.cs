using AutoMapper;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;

namespace TombLauncher.Factories.Profiles;

public class SavegamesProfile : Profile
{
    public SavegamesProfile()
    {
        CreateMap<FileBackup, SavegameBackupDto>()
            .ForMember(dto => dto.MetadataId, opt => opt.MapFrom(v => v.SavegameMetadata != null ? v.SavegameMetadata.Id : 0))
            .ForMember(dto => dto.SlotNumber, opt => opt.MapFrom(v => v.SavegameMetadata != null ? v.SavegameMetadata.SlotNumber : 0))
            .ForMember(dto => dto.SaveNumber, opt => opt.MapFrom(v => v.SavegameMetadata != null ? v.SavegameMetadata.SaveNumber : 0))
            .ForMember(dto => dto.LevelName, opt => opt.MapFrom(v => v.SavegameMetadata != null ? v.SavegameMetadata.LevelName : null))
            .ForMember(dto => dto.GameEngine, opt => opt.MapFrom(v => v.Game!.GameEngine));

        CreateMap<SavegameBackupDto, FileBackup>()
            .ConstructUsing(dto => new FileBackup()
            {
                SavegameMetadata = new SavegameMetadata()
            })
            .ForPath(dto => dto.SavegameMetadata!.Id, opt => opt.MapFrom(o => o.MetadataId))
            .ForPath(dto => dto.SavegameMetadata!.LevelName, opt => opt.MapFrom(o => o.LevelName))
            .ForPath(dto => dto.SavegameMetadata!.SaveNumber, opt => opt.MapFrom(o => o.SaveNumber))
            .ForPath(dto => dto.SavegameMetadata!.SlotNumber, opt => opt.MapFrom(o => o.SlotNumber))
            .ForPath(dto => dto.SavegameMetadata!.FileBackupId, opt => opt.MapFrom(o => o.Id))
            .ForMember(dto => dto.Arguments, opt => opt.Ignore())
            .ForMember(dto => dto.Game, opt => opt.Ignore());

        CreateMap<SavegameMetadata, SavegameBackupDto>()
            .ForMember(dto => dto.Id, opt => opt.MapFrom(savegame => savegame.FileBackup!.Id))
            .ForMember(dto => dto.MetadataId, opt => opt.MapFrom(savegame => savegame.Id))
            .ForMember(dto => dto.FileName, opt => opt.MapFrom(v => v.FileBackup!.FileName))
            .ForMember(dto => dto.Data, opt => opt.MapFrom(v => v.FileBackup!.Data))
            .ForMember(dto => dto.BackedUpOn, opt => opt.MapFrom(v => v.FileBackup!.BackedUpOn))
            .ForMember(dto => dto.FileType, opt => opt.MapFrom(v => v.FileBackup!.FileType))
            .ForMember(dto => dto.GameId, opt => opt.MapFrom(v => v.FileBackup!.GameId))
            .ForMember(dto => dto.Md5, opt => opt.MapFrom(v => v.FileBackup!.Md5))
            .ForMember(dto => dto.GameEngine, opt => opt.MapFrom(v => v.FileBackup!.Game!.GameEngine))
            .ReverseMap();
    }
}