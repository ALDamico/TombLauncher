using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Dtos;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.Core.HardwareInfo;

public class HardwareInfoService
{
    public async Task<HardwareInfoDto> DetectHardware(IProgress<PageBusyState>? progress = null)
    {
        var hardwareInfo = new Hardware.Info.HardwareInfo();
        await Task.Run(() =>
        {
            progress?.Report(new PageBusyState(){IsBusy = true, BusyMessage = "REFRESHING_CPU_INFO".GetLocalizedString()});
            hardwareInfo.RefreshCPUList();
            progress?.Report(new PageBusyState(){IsBusy = true, BusyMessage = "REFRESHING_MEMORY_INFO".GetLocalizedString()});
            hardwareInfo.RefreshMemoryStatus();
            progress?.Report(new PageBusyState(){IsBusy = true, BusyMessage = "REFRESHING_GPU_INFO".GetLocalizedString()});
            hardwareInfo.RefreshVideoControllerList();
        });

        var dto = new HardwareInfoDto()
        {
            TotalRamMb = hardwareInfo.MemoryStatus.TotalPhysical,
            AvailableRamMb = hardwareInfo.MemoryStatus.AvailablePhysical,
            Gpus = hardwareInfo.VideoControllerList.Select(vc => new GpuInfoDto()
                { Name = vc.Name, VRamMb = vc.AdapterRAM, IsVulkanCapable = null }).ToList()
        };
        return dto;
    }
}