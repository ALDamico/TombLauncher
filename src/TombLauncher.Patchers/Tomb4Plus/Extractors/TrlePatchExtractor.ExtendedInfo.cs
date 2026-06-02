using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Extractors;

public partial class TrlePatchExtractor
{
    private void ReadExtendedInfo(BinaryReader reader, bool isExtendedFileSize, bool isUsingRemappedMemory,
        TrlePatchExecutorOutput patchData, PatchBinaryType patchType)
    {
        var metaInfo = new MetaInfo()
        {
            EsseFileLoading = false,
            EsseScriptedParams = false,
            EsseMultipleMirrors = false,
            FurrSupport = false
        };
        patchData.MetaInfo = metaInfo;

        if (!isExtendedFileSize)
            return;

        if (patchType == PatchBinaryType.TrepExe)
        {
            if (!reader.IsNopAtRange(0x000C1000, 0x000C2FFF))
            {
                _logger.LogInformation("FURR support enabled!");
                patchData.MetaInfo.FurrSupport = true;
            }

            patchData.MetaInfo.EsseFileLoading = !(reader.IsNopAtRange(0x000EFBA0, 0x000EFBC8) &&
                                                   reader.IsNopAtRange(0x000EFFE0, 0x000F0002));

            if (patchData.MetaInfo.EsseFileLoading)
            {
                _logger.LogInformation("eSSe file loading enabled!");
                if (!reader.IsNopAtRange(0x000F0010, 0x000F0A3D))
                    patchData.MetaInfo.EsseScriptedParams = true;

                if (!reader.IsNopAtRange(0x000F5E10, 0x000F6113))
                    patchData.MetaInfo.EsseMultipleMirrors = true;

                if (isUsingRemappedMemory)
                {
                    if (!reader.IsNopAtRange(0x000F6550, 0x000F66B8))
                    {
                        patchData.GlobalLevelInfo?.GfxInfo?.ColdBreath = Constants.ColdBreath;
                        patchData.GlobalLevelInfo?.EnvironmentInfo?.RoomColdFlag = Constants.RoomColdFlag;
                        
                        // These are assigned but nothing is done with them in the original Python code. Implemented but 
                        // commented out here
                        /*var coldBreathAnimation103StartFrame = reader.ReadUIntAt(0x000F6639);
                        var coldBreathAnimation110StartFrame = reader.ReadUIntAt(0x000F6645);
                        var coldBreathAnimation222StartFrame = reader.ReadUIntAt(0x000F6653);
                        var coldBreathAnimation263StartFrame = reader.ReadUIntAt(0x000F6661);
                        var coldBreathGeneralCycleLength = reader.ReadUIntAt(0x000F668F);
                        var coldBreathGeneralCycleStartFrame = reader.ReadUIntAt(0x000F66A0);
                        var coldBreathBypass103AnimationSync = reader.ReadByteAt(0x000F663E) == 0x35;*/
                    }
                }
            }
        }
        
        // Draw Legend on Flybys
        if (!reader.IsNopAtRange(0x000EF7C0, 0x000EF7F3))
            patchData.GlobalLevelInfo?.MiscInfo?.DrawLegendOnFlyby = true;
                
        // Show HP bar in Inventory
        if (!reader.IsNopAtRange(0x000EFD90, 0x000EFDCB))
        {
            _logger.LogInformation("Show HP bar in Inventory: true");
        }

        if (patchType == PatchBinaryType.TrepExe)
        {
            // Enable Ricochet SFX
            if (!reader.IsNopAtRange(0x000EE422, 0x000EE43E))
                patchData.GlobalLevelInfo?.MiscInfo?.EnableRicochetSoundEffect = true;
            
            // Enable Revolver Shell Casings
            if (!reader.IsNopAtRange(0x000EFEC0, 0x000EFEDC))
                _logger.LogInformation("Enable Revolver Shell Casings: true");
            
            // Enable Crossbow Shell Casings
            if (!reader.IsNopAtRange(0x000EFFC0, 0x000EFFDA))
                _logger.LogInformation("Enable Crossbow Shell Casings: true");
            
            // Enable Custom Switch Animation OCB
            if (!reader.IsNopAtRange(0x000EFBD0, 0x000EFD2D))
            {
                var miscInfo = patchData.GlobalLevelInfo?.MiscInfo; 
                miscInfo?.TrepSwitchMaker = true;
                _logger.LogInformation("Enable custom switch animation OCB: true");

                miscInfo?.TrepSwitchOnOcb1Anim = reader.ReadShortAt(0x000EFC6E);
                miscInfo?.TrepSwitchOffOcb1Anim = reader.ReadShortAt(0x000EFD00);

                miscInfo?.TrepSwitchOnOcb2Anim = reader.ReadShortAt(0x000EFC00);
                miscInfo?.TrepSwitchOffOcb2Anim = reader.ReadShortAt(0x000EFC96);

                miscInfo?.TrepSwitchOnOcb5Anim = reader.ReadShortAt(0x000EFC42);
                miscInfo?.TrepSwitchOffOcb5Anim = reader.ReadShortAt(0x000EFCD8);

                miscInfo?.TrepSwitchOnOcb6Anim = reader.ReadShortAt(0x000EFC56);
                miscInfo?.TrepSwitchOffOcb6Anim = reader.ReadShortAt(0x000EFCEC);
            }
            
            // Enable Rollingball Smash and Kill
            if (!reader.IsNopAtRange(0x000EEDE0, 0x000EEE17))
                patchData.GlobalLevelInfo?.MiscInfo?.EnableSmashingAndKillingRollingBalls = true;
            _logger.LogInformation("Enable RollingBallSmashAndKill: {Value}", patchData.GlobalLevelInfo?.MiscInfo?.EnableSmashingAndKillingRollingBalls);

            // Enable teeth spikes kill enemies
            if (!reader.IsNopAtRange(0x000EF3F0, 0x000EF406))
                patchData.GlobalLevelInfo?.MiscInfo?.EnableTeethSpikesKillEnemies = true;
            _logger.LogInformation("Enable teeth spikes kill enemies: {Value}", patchData.GlobalLevelInfo?.MiscInfo?.EnableTeethSpikesKillEnemies);

            // Enable Standing Pushables
            if (!reader.IsNopAtRange(0x000EE43F, 0x000EE9DE))
                patchData.GlobalLevelInfo?.MiscInfo?.EnableStandingPushables = true;
            _logger.LogInformation("Enable standing pushables: {Value}", patchData.GlobalLevelInfo?.MiscInfo?.EnableStandingPushables);
        }
        else
        {
            patchData.GlobalLevelInfo?.MiscInfo?.EnableRicochetSoundEffect =
                FlepPatchCheckIfHasGunRicochetEffect(reader).NullIf(false);
        }
    }

    private bool FlepPatchCheckIfHasGunRicochetEffect(BinaryReader reader)
    {
        if (reader.ReadByteAt(0x00033FC6) != 0xE9)
            return false;
        if (reader.ReadByteAt(0x00033FC7) != 0xB5)
            return false;
        if (reader.ReadByteAt(0x00033FC8) != 0x0F)
            return false;
        if (reader.ReadByteAt(0x00033FC9) != 0x3E)
            return false;
        if (reader.ReadByteAt(0x00033FCA) != 0)
            return false;
        if (reader.ReadByteAt(0x00033FCB) != 0x90)
            return false;
        if (reader.ReadByteAt(0x00033FCC) != 0x90)
            return false;
        
        return true;
    }
}