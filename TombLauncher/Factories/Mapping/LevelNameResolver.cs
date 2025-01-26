using AutoMapper;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Factories.Mapping;

public class LevelNameResolver : IValueResolver<FileBackupDto, SavegameBackupDto, string>
{
    public LevelNameResolver(SavegameHeaderProxy proxiedHeader)
    {
        ProxiedHeader = proxiedHeader;
    }

    public string Resolve(FileBackupDto source, SavegameBackupDto destination, string destMember,
        ResolutionContext context)
    {
        ProxiedHeader.Filepath = source.FileName;
        return ProxiedHeader.LevelName;
    }

    public SavegameHeaderProxy ProxiedHeader { get; set; }
}