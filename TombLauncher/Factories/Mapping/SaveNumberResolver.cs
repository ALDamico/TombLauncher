using AutoMapper;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Factories.Mapping;

public class SaveNumberResolver : IValueResolver<FileBackupDto, SavegameBackupDto, int>
{
    public SaveNumberResolver(SavegameHeaderProxy proxiedHeader)
    {
        ProxiedHeader = proxiedHeader;
    }

    public int Resolve(FileBackupDto source, SavegameBackupDto destination, int destMember, ResolutionContext context)
    {
        ProxiedHeader.Filepath = source.FileName;
        return ProxiedHeader.SaveNumber;
    }

    public SavegameHeaderProxy ProxiedHeader { get; set; }
}