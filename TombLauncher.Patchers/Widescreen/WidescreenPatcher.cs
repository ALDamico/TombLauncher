using TombLauncher.Contracts;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Patchers;
using TombLauncher.Patchers.Widescreen.Ui.View;

namespace TombLauncher.Patchers.Widescreen;

public class WidescreenPatcher : IPatcher
{
    public Version Version => new Version(1, 0);
    public string Author => "Tomb Launcher developers";

    public string Description =>
        @"This patcher allows you to modify the game executable in order to support aspect ratios other than 4:3.
It is a port of Ed Kurlyak's Widescreen patch with a few improvements, such as:
 - Can apply the change of aspect ratio, field of view, and camera ";

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
                var fileInfo = new FileInfo(Path.Combine(targetFolder, detectorResult.ExecutablePath));
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
        var patchResult = new PatchResult()
        {
            IsSuccessful = false
        };

        var changesToApply = 0;
        if (parameters.UpdateAspectRatio)
        {
            changesToApply++;
        }

        if (parameters.UpdateCameraDistance)
        {
            changesToApply++;
        }

        if (parameters.UpdateFov)
        {
            changesToApply++;
        }


        foreach (var affectedFile in executablePath.AffectedFiles.Where(f => f.ChangeType == ChangeType.BinaryEdit))
        {
            var bytes = await File.ReadAllBytesAsync(affectedFile.Filename);
            if (Math.Abs(parameters.OriginalAspectRatio - 0) < float.Epsilon)
            {
                parameters.OriginalAspectRatio = FourByThreeAspectRatio;
            }

            // In an unpatched executable should be 0xAB 0xAA 0xAA 0X3F
            var originalAspectRatioBytes = BitConverter.GetBytes(parameters.OriginalAspectRatio);
            var targetAspectRatioBytes = BitConverter.GetBytes(parameters.TargetAspectRatio);

            if (parameters is { UpdateAspectRatio: true, TargetAspectRatio: > 0 })
            {
                var i = 0;
                do
                {
                    if (bytes[i] == originalAspectRatioBytes[0] && bytes[i + 1] == originalAspectRatioBytes[1] &&
                        bytes[i + 2] == originalAspectRatioBytes[2] && bytes[i + 3] == originalAspectRatioBytes[3])
                    {
                        affectedFile.StartOffset = i;
                        bytes[i] = targetAspectRatioBytes[0];
                        bytes[i + 1] = targetAspectRatioBytes[1];
                        bytes[i + 2] = targetAspectRatioBytes[2];
                        bytes[i + 3] = targetAspectRatioBytes[3];

                        patchResult.AffectedFiles.Add(new FileChange()
                        {
                            Filename = affectedFile.Filename,
                            ChangeType = ChangeType.BinaryEdit,
                            NewSize = bytes.Length,
                            StartOffset = i,
                            EndOffset = i + 3,
                            OriginalSize = bytes.Length,
                            Message = "Aspect ratio patch applied."
                        });
                        break;
                    }

                    i++;
                } while (i < bytes.Length - 4);
            }

            if (parameters is { UpdateCameraDistance: true, TargetCameraDistance: > 0 })
            {
                var originalDistanceBytes = BitConverter.GetBytes(parameters.OriginalCameraDistance);
                var targetDistanceBytes = BitConverter.GetBytes(parameters.TargetCameraDistance);
                var i = 0;
                do
                {
                    if (bytes[i] == 0xC7 && bytes[i + 1] == 0x5)
                    {
                        if (bytes[i + 6] == originalDistanceBytes[0] && bytes[i + 6 + 1] == originalDistanceBytes[1])
                        {
                            bytes[i + 6] = targetDistanceBytes[0];
                            bytes[i + 6 + 1] = targetDistanceBytes[1];

                            patchResult.AffectedFiles.Add(new FileChange()
                            {
                                Filename = affectedFile.Filename,
                                ChangeType = ChangeType.BinaryEdit,
                                Message = "Camera distance updated",
                                StartOffset = i,
                                EndOffset = i + 3,
                                NewSize = bytes.Length,
                                OriginalSize = bytes.Length
                            });
                        }
                    }

                    i++;
                } while (i < bytes.Length - 7);
            }

            if (parameters is { UpdateAspectRatio: true, HorizontalResolution: > 0 })
            {
                var i = 0;
                do
                {
                    if (bytes[i + 5] == 0x99)
                    {
                        var Ffov = (float)parameters.HorizontalResolution;
                        var res = Ffov / parameters.OriginalAspectRatio;
                        var intFov = (int)res;
                        var TFov = intFov << 8 | 0xB8;
                        var bytesToWrite = BitConverter.GetBytes(TFov);
                        bytes[i] = bytesToWrite[0];
                        bytes[i + 1] = bytesToWrite[1];
                        bytes[i + 2] = bytesToWrite[2];
                        bytes[i + 3] = bytesToWrite[3];
                        patchResult.AffectedFiles.Add(new FileChange()
                        {
                            Filename = affectedFile.Filename,
                            Message = "FoV correction applied",
                            ChangeType = ChangeType.BinaryEdit,
                            StartOffset = i,
                            EndOffset = i + 3,
                            NewSize = bytes.Length,
                            OriginalSize = bytes.Length
                        });
                    }
                } while (i < bytes.Length - 5);
            }


            await File.WriteAllBytesAsync(affectedFile.Filename, bytes);
        }

        if (patchResult.AffectedFiles.Count == changesToApply)
        {
            patchResult.IsSuccessful = true;
            patchResult.Message = "All changes applied";
        }
        else
        {
            patchResult.Message = $"Expected {changesToApply} changes, but found {patchResult.AffectedFiles.Count}.";
        }

        return patchResult;
    }

    public TombRaiderEngineDetector EngineDetector { get; set; }
    public IPatcherUi GetUi()
    {
        return new WidescreenPatcherUi();
    }

    private const float FourByThreeAspectRatio = 4.0F / 3;
}