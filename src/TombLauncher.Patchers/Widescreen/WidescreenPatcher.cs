using TombLauncher.Contracts.EngineDetectors;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Patchers;

namespace TombLauncher.Patchers.Widescreen;

public class WidescreenPatcher : IPatcher
{
    public async Task<PatchResult> DetectChanges(string targetFolder)
    {
        var detectorResult = EngineDetector.Detect(targetFolder);
        var engine = detectorResult.GameEngine;
        switch (engine)
        {
            case GameEngine.TombRaider1:
            case GameEngine.TombRaider2:
            case GameEngine.TombRaider3:
            case GameEngine.TombRaider4:
            case GameEngine.TombRaider5:
                var fileInfo = new FileInfo(Path.Combine(targetFolder, detectorResult.ExecutablePath!));
                return new PatchResult()
                {
                    IsSuccessful = true,
                    AffectedFiles = new List<FileChange>()
                    {
                        new FileChange()
                        {
                            Filename = fileInfo.FullName,
                            ChangeType = ChangeType.BinaryEdit,
                            OriginalSize = fileInfo.Length
                        }
                    }
                };
            default:
                return new PatchResult()
                {
                    IsSuccessful = false,
                    Message = "This game engine is not compatible with the widescreen patch"
                };
        }
    }

    public async Task<PatchResult> ApplyPatch(string targetFolder, IPatchParameters parameters)
    {
        if (parameters is not WidescreenPatcherParameters parms)
        {
            throw new ArgumentException("Incompatible parameters!", nameof(parameters));
        }
        var tempResult = await DetectChanges(targetFolder);
        if (!tempResult.IsSuccessful)
        {
            return tempResult;
        }

        return await PatchExecutable(tempResult, parms);
    }

    private async Task<PatchResult> PatchExecutable(PatchResult executablePath, WidescreenPatcherParameters parameters)
    {
        foreach (var affectedFile in executablePath.AffectedFiles.Where(f => f.ChangeType == ChangeType.BinaryEdit))
        {
            var bytes = await File.ReadAllBytesAsync(affectedFile.Filename);

            ApplyAspectRatioCorrection(parameters, bytes, affectedFile);
            ApplyCameraDistanceCorrection(parameters, bytes, affectedFile);
            ApplyFovCorrection(parameters, bytes, affectedFile);
            Apply60Fps(parameters, bytes, affectedFile);

            await File.WriteAllBytesAsync(affectedFile.Filename, bytes);
        }

        return executablePath;
    }

    private static void Apply60Fps(WidescreenPatcherParameters parameters, byte[] bytes, FileChange affectedFile)
    {
        if (!parameters.Update60Fps || parameters.Engine is not (GameEngine.TombRaider2 or GameEngine.TombRaider3))
            return;

        var i = 0;
        do
        {
            if (bytes[i] == 0x8B && bytes[i + 1] == 0xF8 && bytes[i + 2] == 0x83 && bytes[i + 3] == 0xFF &&
                bytes[i + 4] == 0x02)
            {
                bytes[i + 4] = 0x00;
                affectedFile.Offset = i + 4;
                break;
            }

            i++;
        } while (i < bytes.Length - 5);
    }

    private static void ApplyCameraDistanceCorrection(WidescreenPatcherParameters parameters, byte[] bytes,
        FileChange affectedFile)
    {
        if (parameters is not { UpdateCameraDistance: true, TargetCameraDistance: > 0 })
            return;

        var cameraDistanceValue = BitConverter.GetBytes((short)parameters.TargetCameraDistance);

        var i = 0;
        do
        {
            if (bytes[i] == 0xC7 && bytes[i + 1] == 0x05 && bytes[i + 6] == 0x00 && bytes[i + 7] == 0x06)
            {
                affectedFile.Offset = i + 6;
                bytes[i + 6] = cameraDistanceValue[0];
                bytes[i + 7] = cameraDistanceValue[1];
            }
            i++;
        } while (i < bytes.Length - 7);
    }

    private static void ApplyFovCorrection(WidescreenPatcherParameters parameters, byte[] bytes,
        FileChange affectedFile)
    {
        if (!parameters.UpdateFov)
            return;

        var i = 0;
        do
        {
            if (bytes[i] == 0xA1 && bytes[i + 5] == 0x99)
            {
                affectedFile.Offset = i;
                var fov = (int)(parameters.TargetFov / FourByThreeAspectRatio) << 8 | 0xB8;
                var fovBytes = BitConverter.GetBytes(fov);
                for (var j = 0; j < 4; j++)
                {
                    bytes[j + i] = fovBytes[j];
                }
            }
            i++;
        } while (i < bytes.Length - 5);
    }

    private static void ApplyAspectRatioCorrection(WidescreenPatcherParameters parameters, byte[] bytes,
        FileChange affectedFile)
    {
        if (parameters is not { UpdateAspectRatio: true, TargetAspectRatio: > 0 }) 
            return;
        
        // In an unpatched executable should be 0xAB 0xAA 0xAA 0X3F
        var originalAspectRatioBytes = BitConverter.GetBytes(FourByThreeAspectRatio);
        var targetAspectRatioBytes = BitConverter.GetBytes(parameters.TargetAspectRatio);
        var i = 0;
        do
        {
            if (bytes[i] == originalAspectRatioBytes[0] && bytes[i + 1] == originalAspectRatioBytes[1] &&
                bytes[i + 2] == originalAspectRatioBytes[2] && bytes[i + 3] == originalAspectRatioBytes[3])
            {
                affectedFile.Offset = i;
                bytes[i] = targetAspectRatioBytes[0];
                bytes[i + 1] = targetAspectRatioBytes[1];
                bytes[i + 2] = targetAspectRatioBytes[2];
                bytes[i + 3] = targetAspectRatioBytes[3];
                break;
            }

            i++;
        } while (i < bytes.Length - 4);
    }

    public required IEngineDetector EngineDetector { get; set; }
    private const float FourByThreeAspectRatio = 4.0F / 3;
}
