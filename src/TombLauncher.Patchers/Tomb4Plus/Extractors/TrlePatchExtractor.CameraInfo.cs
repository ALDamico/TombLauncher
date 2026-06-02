using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Extractors;

public partial class TrlePatchExtractor
{
    private CameraInfo? ReadCameraInfo(BinaryReader reader, PatchBinaryType patchType)
    {
        if (patchType != PatchBinaryType.TrepExe)
            return null;

        var cameraInfo = new CameraInfo();

        const short defaultCameraVerticalOrientation = -1820;
        const short defaultLookCameraDistance = -1024;
        const short defaultAddOnBattleCameraTop = 256;
        const short defaultCameraSpeed = 10;
        const short defaultCameraDistance = 1536;

        cameraInfo.ChaseCameraVerticalOrientation =
            reader.ReadShortAt(0x00042DB9).NullIf(defaultCameraVerticalOrientation);
        cameraInfo.LookCameraDistance = reader.ReadShortAt(0x0004387C).NullIf(defaultLookCameraDistance);
        cameraInfo.AddOnBattleCameraTop = reader.ReadShortAt(0x000444C5).NullIf(defaultAddOnBattleCameraTop);
        cameraInfo.CameraSpeed = reader.ReadShortAt(0x00044574).NullIf(defaultCameraSpeed);
        cameraInfo.NormalCameraDistance = reader.ReadShortAt(0x0004459C).NullIf(defaultCameraDistance);
        
        if (reader.ReadSByteAt(0x0002D6FE) == 0 && reader.ReadSByteAt(0x0002D729) == 0 && reader.ReadSByteAt(0x0002D7EE) != 0)
            _logger.LogInformation("Unify normal and battle camera: True");

        if (cameraInfo.HasAnyValue())
            return cameraInfo;

        return null;
    }
}