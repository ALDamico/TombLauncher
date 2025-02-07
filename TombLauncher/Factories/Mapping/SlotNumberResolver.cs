using AutoMapper;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Factories.Mapping;

public class SlotNumberResolver : IValueResolver<FileBackupDto, SavegameBackupDto, int>
{
    public SlotNumberResolver(SavegameHeaderProxy proxiedHeader)
    {
        ProxiedHeader = proxiedHeader;
    }

    public int Resolve(FileBackupDto source, SavegameBackupDto destination, int destMember, ResolutionContext context)
    {
        ProxiedHeader.Filepath = source.FileName;
        return ProxiedHeader.SlotNumber;
    }

    public SavegameHeaderProxy ProxiedHeader { get; set; }
}