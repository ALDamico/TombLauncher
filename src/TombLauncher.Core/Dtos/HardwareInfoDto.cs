namespace TombLauncher.Core.Dtos;

public class HardwareInfoDto
{
    public ulong AvailableRamMb { get; init; }
    public ulong TotalRamMb { get; init; }
    public IReadOnlyList<GpuInfoDto> Gpus { get; init; } = [];
}

public class GpuInfoDto
{
    public required string Name { get; init; }
    public ulong VRamMb  { get; init; }
    public bool? IsVulkanCapable { get; set; } // If null, not yet checked
}