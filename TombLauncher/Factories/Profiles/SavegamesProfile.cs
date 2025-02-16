using AutoMapper;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Savegames;
using TombLauncher.Core.Savegames.HeaderReaders;
using TombLauncher.Core.Utils;
using TombLauncher.Data.Models;
using TombLauncher.Factories.Mapping;

namespace TombLauncher.Factories.Profiles;

public class SavegamesProfile : Profile
{
    public SavegamesProfile()
    {
        CreateMap<FileBackup, SavegameBackupDto>()
            .ForMember(dto => dto.MetadataId, opt => opt.MapFrom(v => v.SavegameMetadata.Id))
            .ForMember(dto => dto.SlotNumber, opt => opt.MapFrom(v => v.SavegameMetadata.SlotNumber))
            .ForMember(dto => dto.SaveNumber, opt => opt.MapFrom(v => v.SavegameMetadata.SaveNumber))
            .ForMember(dto => dto.LevelName, opt => opt.MapFrom(v => v.SavegameMetadata.LevelName))
            .ForMember(dto => dto.GameEngine, opt => opt.MapFrom(v => v.Game.GameEngine));
        
        CreateMap<SavegameBackupDto, FileBackup>()
            .ConstructUsing(dto => new FileBackup()
            {
                SavegameMetadata = new SavegameMetadata()
            })
            .ForPath(dto => dto.SavegameMetadata.Id, opt => opt.MapFrom(o => o.MetadataId))
            .ForPath(dto => dto.SavegameMetadata.LevelName, opt => opt.MapFrom(o => o.LevelName))
            .ForPath(dto => dto.SavegameMetadata.SaveNumber, opt => opt.MapFrom(o => o.SaveNumber))
            .ForPath(dto => dto.SavegameMetadata.SlotNumber, opt => opt.MapFrom(o => o.SlotNumber))
            .ForPath(dto => dto.SavegameMetadata.FileBackupId, opt => opt.MapFrom(o => o.Id))
            .ForMember(dto => dto.Arguments, opt => opt.Ignore())
            .ForMember(dto => dto.Game, opt => opt.Ignore());
        
        CreateMap<SavegameMetadata, SavegameBackupDto>()
            .ForMember(dto => dto.Id, opt => opt.MapFrom(savegame => savegame.FileBackup.Id))
            .ForMember(dto => dto.MetadataId, opt => opt.MapFrom(savegame => savegame.Id))
            .ForMember(dto => dto.FileName, opt => opt.MapFrom(v => v.FileBackup.FileName))
            .ForMember(dto => dto.Data, opt => opt.MapFrom(v => v.FileBackup.Data))
            .ForMember(dto => dto.BackedUpOn, opt => opt.MapFrom(v => v.FileBackup.BackedUpOn))
            .ForMember(dto => dto.FileType, opt => opt.MapFrom(v => v.FileBackup.FileType))
            .ForMember(dto => dto.GameId, opt => opt.MapFrom(v => v.FileBackup.GameId))
            .ForMember(dto => dto.Md5, opt => opt.MapFrom(v => v.FileBackup.Md5))
            .ForMember(dto => dto.GameEngine, opt => opt.MapFrom(v => v.FileBackup.Game.GameEngine))
            .ReverseMap();

        var mappingHeaderReader = new SavegameHeaderReader();
        var headerProxy = new SavegameHeaderProxy(mappingHeaderReader);

        var levelNameResolver = new LevelNameResolver(headerProxy);
        var slotNumberResolver = new SlotNumberResolver(headerProxy);
        var saveNumberResolver = new SaveNumberResolver(headerProxy);

        CreateMap<FileBackupDto, SavegameBackupDto>()
            .ForMember(m => m.Md5,
                opt => opt.MapFrom(file => Md5Utils.ComputeMd5Hash(file.Data).GetAwaiter().GetResult()))
            .ForMember(m => m.LevelName, opt => opt.MapFrom(levelNameResolver))
            .ForMember(m => m.SlotNumber, opt => opt.MapFrom(slotNumberResolver))
            .ForMember(m => m.SaveNumber, opt => opt.MapFrom(saveNumberResolver))
            .ForMember(m => m.MetadataId, opt => opt.Ignore())
            .ForMember(m => m.GameEngine, opt => opt.Ignore());
    }
}