using TombLauncher.Contracts;
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
            
            await File.WriteAllBytesAsync(affectedFile.Filename, bytes);
        }

        return executablePath;
    }

    public TombRaiderEngineDetector EngineDetector { get; set; }
    private const float FourByThreeAspectRatio = 4.0F / 3;
}