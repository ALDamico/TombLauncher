using System.Text;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Parsers;

public class EsseParser
{
    private readonly ILogger<EsseParser> _logger;

    public EsseParser(ILogger<EsseParser> logger)
    {
        _logger = logger;
    }

    private void ReadData(BinaryReader reader, Dictionary<string, object> d, string keyName, int baseOffset, int offset,
        int maxBlockSize, string type, int stringWidth, object defaultValue)
    {
        object? data;

        reader.Seek(baseOffset + offset);

        switch (type)
        {
            case "FLOAT":
                if (offset + 4 <= maxBlockSize)
                    data = reader.ReadSingle();
                else
                    data = BitConverter.Int32BitsToSingle((int)defaultValue);
                break;
            case "BOOL":
                if (offset + 1 <= maxBlockSize)
                    data = reader.ReadByte() != 0;
                else
                    data = (bool)defaultValue;
                break;
            case "BYTE":
                if (offset + 1 <= maxBlockSize)
                    data = reader.ReadByte();
                else
                    data = (byte)defaultValue;
                break;
            case "BYTEI":
                if (offset + 1 <= maxBlockSize)
                    data = reader.ReadSByte();
                else
                    data = (sbyte)defaultValue;
                break;
            case "WORD":
                if (offset + 2 <= maxBlockSize)
                    data = reader.ReadUInt16();
                else
                    data = (ushort)defaultValue;
                break;
            case "WORDI":
                if (offset + 2 <= maxBlockSize)
                    data = reader.ReadInt16();
                else
                    data = (short)defaultValue;
                break;
            case "DWORD":
                if (offset + 4 <= maxBlockSize)
                    data = reader.ReadUInt32();
                else
                    data = (uint)defaultValue;
                break;
            case "HEX":
                if (offset + 3 <= maxBlockSize)
                {
                    var bytes = reader.ReadBytes(3);
                    data = $"#{bytes[0]:x2}{bytes[1]:x2}{bytes[2]:x2}";
                }
                else
                    data = "#000000";

                break;
            case "STRING":
                data = Encoding.ASCII.GetString(
                    reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position)));
                break;
            case "FIXED_STRING":
                if (offset + stringWidth <= maxBlockSize)
                    data = Encoding.ASCII.GetString(reader.ReadBytes(stringWidth));
                else
                    data = defaultValue.ToString();
                break;
            default:
                data = defaultValue;
                break;
        }

        if (data != null)
            d[keyName] = data;
    }

    public List<LevelData>? ReadBinaryFile(string filePath, TrlePatchExecutorOutput patchData)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("The file {FilePath} does not exist", filePath);
            return null;
        }

        using var reader = new BinaryReader(File.OpenRead(filePath));

        var levelCount = reader.ReadByte();
        reader.Seek(1);
        var levelBlockSize = reader.ReadInt32();

        var levelArray = new List<LevelData>();

        for (var i = 0; i < levelCount; i++)
        {
            var data = new Dictionary<string, object>();
            var baseOffset = 5 + levelBlockSize * i;
            reader.Seek(0);

            ReadData(reader, data, "DD", baseOffset, 0, levelBlockSize, "FLOAT", 0, 20480);
            ReadData(reader, data, "DF", baseOffset, 4, levelBlockSize, "FLOAT", 0, 12288);
            ReadData(reader, data, "VolFX", baseOffset, 8, levelBlockSize, "BOOL", 0, true);

            // Weapon Data
            ReadData(reader, data, "PistolDmg", baseOffset, 9, levelBlockSize, "BYTE", 0, 1);
            ReadData(reader, data, "PistolRate", baseOffset, 10, levelBlockSize, "BYTE", 0, 9);
            ReadData(reader, data, "PistolDisp", baseOffset, 10, levelBlockSize, "BYTE", 0, 5);
            ReadData(reader, data, "PistolFlashDur", baseOffset, 12, levelBlockSize, "BYTE", 0, 3);
            ReadData(reader, data, "UziDmg", baseOffset, 13, levelBlockSize, "BYTE", 0, 1);
            ReadData(reader, data, "UziRate", baseOffset, 14, levelBlockSize, "BYTE", 0, 3);
            ReadData(reader, data, "UziDisp", baseOffset, 15, levelBlockSize, "BYTE", 0, 5);
            ReadData(reader, data, "UziFlashDur", baseOffset, 16, levelBlockSize, "BYTE", 0, 3);
            ReadData(reader, data, "RevolverDmg", baseOffset, 17, levelBlockSize, "BYTE", 0, 21);
            ReadData(reader, data, "RevolverRate", baseOffset, 18, levelBlockSize, "BYTE", 0, 16);
            ReadData(reader, data, "RevolverDisp", baseOffset, 19, levelBlockSize, "BYTE", 0, 2);
            ReadData(reader, data, "RevolverFlashDur", baseOffset, 20, levelBlockSize, "BYTE", 0, 3);
            ReadData(reader, data, "ShotgunDmg", baseOffset, 21, levelBlockSize, "BYTE", 0, 3);
            ReadData(reader, data, "ShotgunFlashDur", baseOffset, 22, levelBlockSize, "BYTE", 0, 3);
            ReadData(reader, data, "Gravity", baseOffset, 23, levelBlockSize, "BYTE", 0, 5);
            ReadData(reader, data, "DFThresh", baseOffset, 24, levelBlockSize, "FLOAT", 0, 20480);
            ReadData(reader, data, "WUVScroll", baseOffset, 28, levelBlockSize, "BYTEI", 0, 7);

            // Waterfall Mist
            ReadData(reader, data, "MistRGB1", baseOffset, 30, levelBlockSize, "HEX", 0, "#c0c0c0");
            ReadData(reader, data, "MistRGB2", baseOffset, 33, levelBlockSize, "HEX", 0, "#808080");
            ReadData(reader, data, "MistSize", baseOffset, 36, levelBlockSize, "BYTE", 0, 12);
            ReadData(reader, data, "MistDensity", baseOffset, 37, levelBlockSize, "BYTE", 0, 6);
            ReadData(reader, data, "MistAmount", baseOffset, 38, levelBlockSize, "BYTE", 0, 4);

            // Lights
            ReadData(reader, data, "PistolFlashArea", baseOffset, 39, levelBlockSize, "BYTE", 0, 10);
            ReadData(reader, data, "RevolverFlashArea", baseOffset, 40, levelBlockSize, "BYTE", 0, 12);
            ReadData(reader, data, "ShotgunFlashArea", baseOffset, 41, levelBlockSize, "BYTE", 0, 12);
            ReadData(reader, data, "FlameEmitterArea", baseOffset, 42, levelBlockSize, "BYTE", 0, 16);
            ReadData(reader, data, "BlinkingLightRGB", baseOffset, 43, levelBlockSize, "HEX", 0, "#ffc010");
            ReadData(reader, data, "BlinkingLightArea", baseOffset, 46, levelBlockSize, "BYTE", 0, 16);

            // HP
            ReadData(reader, data, "HealthAtStartup", baseOffset, 47, levelBlockSize, "WORD", 0, 1000);
            ReadData(reader, data, "AirAtStartup", baseOffset, 49, levelBlockSize, "WORD", 0, 1800);

            // Weapons
            ReadData(reader, data, "ShotgunShots", baseOffset, 51, levelBlockSize, "BYTE", 0, 6);
            ReadData(reader, data, "CrossbowBoltSpeed", baseOffset, 52, levelBlockSize, "WORD", 0, 512);
            ReadData(reader, data, "CrossbowDmg", baseOffset, 54, levelBlockSize, "WORD", 0, 5);
            ReadData(reader, data, "ExplosiveDamage", baseOffset, 55, levelBlockSize, "BYTE", 0, 30);
            ReadData(reader, data, "CrossbowFlags", baseOffset, 56, levelBlockSize, "HEX", 0, "#010203");
            ReadData(reader, data, "GrenadeFlags", baseOffset, 59, levelBlockSize, "HEX", 0, "#010203");
            ReadData(reader, data, "GrenadeTimeout", baseOffset, 62, levelBlockSize, "WORD", 0, 120);
            ReadData(reader, data, "GrenadeWeight", baseOffset, 64, levelBlockSize, "BYTE", 0, 3);
            ReadData(reader, data, "GrenadeLaunchPower", baseOffset, 65, levelBlockSize, "WORD", 0, 128);
            ReadData(reader, data, "GrenadeOneTouch", baseOffset, 67, levelBlockSize, "BOOL", 0, false);
            ReadData(reader, data, "GrenadeRotation", baseOffset, 68, levelBlockSize, "BOOL", 0, true);
            ReadData(reader, data, "PistolTargDistance", baseOffset, 69, levelBlockSize, "WORD", 0, 8192);
            ReadData(reader, data, "UziTargDistance", baseOffset, 71, levelBlockSize, "WORD", 0, 8192);
            ReadData(reader, data, "RevolverTargDistance", baseOffset, 73, levelBlockSize, "WORD", 0, 8192);
            ReadData(reader, data, "ShotgunTargDistance", baseOffset, 75, levelBlockSize, "WORD", 0, 8192);
            ReadData(reader, data, "GrenadeTargDistance", baseOffset, 77, levelBlockSize, "WORD", 0, 8192);
            ReadData(reader, data, "CrossbowTargDistance", baseOffset, 79, levelBlockSize, "WORD", 0, 8192);

            // Traps
            ReadData(reader, data, "JobySpikesSpeed", baseOffset, 81, levelBlockSize, "BYTE", 0, 3);
            ReadData(reader, data, "NormalSpikesTimer", baseOffset, 82, levelBlockSize, "WORD", 0, 64);
            ReadData(reader, data, "NormalSpikesSpeed", baseOffset, 84, levelBlockSize, "WORD", 0, 128);
            ReadData(reader, data, "DartsInterval", baseOffset, 86, levelBlockSize, "WORD", 0, 24);
            ReadData(reader, data, "DartsSpeed", baseOffset, 88, levelBlockSize, "WORD", 0, 256);
            ReadData(reader, data, "DartsRGB", baseOffset, 90, levelBlockSize, "HEX", 0, "#783c14");
            ReadData(reader, data, "BoulderGravity", baseOffset, 93, levelBlockSize, "BYTE", 0, 6);
            ReadData(reader, data, "ConductorInterval", baseOffset, 94, levelBlockSize, "BYTE", 0, 63);

            // Weapon Names
            ReadData(reader, data, "PistolsName", baseOffset, 95, levelBlockSize, "FIXED_STRING", 26, "Pistols");
            ReadData(reader, data, "UzisName", baseOffset, 121, levelBlockSize, "FIXED_STRING", 26, "Uzis");
            ReadData(reader, data, "ShotgunName", baseOffset, 147, levelBlockSize, "FIXED_STRING", 26, "Shotgun");
            ReadData(reader, data, "RevolverName", baseOffset, 173, levelBlockSize, "FIXED_STRING", 26, "Revolver");
            ReadData(reader, data, "RevolverLaserName", baseOffset, 199, levelBlockSize, "FIXED_STRING", 26,
                "Revolver + LaserSight");
            ReadData(reader, data, "CrossbowName", baseOffset, 225, levelBlockSize, "FIXED_STRING", 26, "Crossbow");
            ReadData(reader, data, "CrossbowLaserName", baseOffset, 251, levelBlockSize, "FIXED_STRING", 26,
                "Crossbow + LaserSight");
            ReadData(reader, data, "GrenadeName", baseOffset, 277, levelBlockSize, "FIXED_STRING", 26, "Grenade Gun");

            // Ammo Names
            ReadData(reader, data, "SGNormAmmoName", baseOffset, 303, levelBlockSize, "FIXED_STRING", 26,
                "Shotgun Normal Ammo");
            ReadData(reader, data, "SGWideAmmoName", baseOffset, 329, levelBlockSize, "FIXED_STRING", 26,
                "Shotgun Wideshot Ammo");
            ReadData(reader, data, "GrenadeNormAmmoName", baseOffset, 355, levelBlockSize, "FIXED_STRING", 26,
                "Grenade Gun Normal Ammo");
            ReadData(reader, data, "GrenadeSuperAmmoName", baseOffset, 381, levelBlockSize, "FIXED_STRING", 26,
                "Grenade Gun Super Ammo");
            ReadData(reader, data, "GrenadeFlashAmmoName", baseOffset, 407, levelBlockSize, "FIXED_STRING", 26,
                "Grenade Gun Flash Ammo");
            ReadData(reader, data, "XBowNormAmmoName", baseOffset, 433, levelBlockSize, "FIXED_STRING", 26,
                "Crossbow Normal Ammo");
            ReadData(reader, data, "XBowPoisonAmmoName", baseOffset, 459, levelBlockSize, "FIXED_STRING", 26,
                "Crossbow Poison Ammo");
            ReadData(reader, data, "XBowExplAmmoName", baseOffset, 485, levelBlockSize, "FIXED_STRING", 26,
                "Crossbow Explosive Ammo");
            ReadData(reader, data, "RevolverAmmoName", baseOffset, 511, levelBlockSize, "FIXED_STRING", 26,
                "Revolver Ammo");
            ReadData(reader, data, "UzisAmmoName", baseOffset, 537, levelBlockSize, "FIXED_STRING", 26, "Uzis Ammo");
            ReadData(reader, data, "PistolsAmmoName", baseOffset, 563, levelBlockSize, "FIXED_STRING", 26,
                "Pistols Ammo");

            // Others
            ReadData(reader, data, "LasersightName", baseOffset, 589, levelBlockSize, "FIXED_STRING", 26,
                "Laser-Sight");
            ReadData(reader, data, "LargeMedkitName", baseOffset, 615, levelBlockSize, "FIXED_STRING", 26,
                "Large Medkit");
            ReadData(reader, data, "SmallMedkitName", baseOffset, 641, levelBlockSize, "FIXED_STRING", 26,
                "Small Medkit");
            ReadData(reader, data, "BinocularsName", baseOffset, 667, levelBlockSize, "FIXED_STRING", 26, "Binoculars");
            ReadData(reader, data, "FlaresName", baseOffset, 693, levelBlockSize, "FIXED_STRING", 26, "Flares");
            ReadData(reader, data, "CrowbarName", baseOffset, 719, levelBlockSize, "FIXED_STRING", 26, "Crowbar");

            // Object parameters
            ReadData(reader, data, "FallingBlockTimeout", baseOffset, 745, levelBlockSize, "WORD", 0, 60);
            ReadData(reader, data, "FallingBlockTremble", baseOffset, 747, levelBlockSize, "WORD", 0, 1023);
            ReadData(reader, data, "RaisingBlockHeight", baseOffset, 749, levelBlockSize, "WORD", 0, 1024);
            ReadData(reader, data, "TwoBlockGoDown", baseOffset, 751, levelBlockSize, "BOOL", 0, false);
            ReadData(reader, data, "TwoBlockDeprDist", baseOffset, 752, levelBlockSize, "WORD", 0, 128);
            ReadData(reader, data, "TwoBlockDeprSpeed", baseOffset, 754, levelBlockSize, "BYTE", 0, 4);
            ReadData(reader, data, "TwoBlockReprSpeed", baseOffset, 755, levelBlockSize, "BYTE", 0, 4);

            // Physics
            ReadData(reader, data, "SwimSpeed", baseOffset, 756, levelBlockSize, "WORD", 0, 200);

            // Enemy HP
            ReadData(reader, data, "SkeletonHP", baseOffset, 758, levelBlockSize, "WORDI", 0, 15);
            ReadData(reader, data, "Baddy1HP", baseOffset, 760, levelBlockSize, "WORDI", 0, 25);
            ReadData(reader, data, "Baddy2HP", baseOffset, 762, levelBlockSize, "WORDI", 0, 35);
            ReadData(reader, data, "ScorpionHP", baseOffset, 764, levelBlockSize, "WORDI", 0, 80);
            ReadData(reader, data, "MummyHP", baseOffset, 766, levelBlockSize, "WORDI", 0, 15);
            ReadData(reader, data, "KnightTemplarHP", baseOffset, 768, levelBlockSize, "WORDI", 0, 15);
            ReadData(reader, data, "SphinxHP", baseOffset, 770, levelBlockSize, "WORDI", 0, 1000);
            ReadData(reader, data, "SethHP", baseOffset, 772, levelBlockSize, "WORDI", 0, 500);
            ReadData(reader, data, "HorsemenHP", baseOffset, 774, levelBlockSize, "WORDI", 0, 25);
            ReadData(reader, data, "HammerheadHP", baseOffset, 776, levelBlockSize, "WORDI", 0, 45);
            ReadData(reader, data, "CrocHP", baseOffset, 778, levelBlockSize, "WORDI", 0, 36);
            ReadData(reader, data, "MutantHP", baseOffset, 780, levelBlockSize, "WORDI", 0, 15);
            ReadData(reader, data, "GuideHP", baseOffset, 782, levelBlockSize, "WORDI", 0, -16384);
            ReadData(reader, data, "Demigod1HP", baseOffset, 784, levelBlockSize, "WORDI", 0, 200);
            ReadData(reader, data, "Demigod2HP", baseOffset, 786, levelBlockSize, "WORDI", 0, 200);
            ReadData(reader, data, "Demigod3HP", baseOffset, 788, levelBlockSize, "WORDI", 0, 200);
            ReadData(reader, data, "TroopsHP", baseOffset, 790, levelBlockSize, "WORDI", 0, 40);
            ReadData(reader, data, "SASHP", baseOffset, 792, levelBlockSize, "WORDI", 0, 40);
            ReadData(reader, data, "HarpyHP", baseOffset, 794, levelBlockSize, "WORDI", 0, 60);
            ReadData(reader, data, "WildBoarHP", baseOffset, 796, levelBlockSize, "WORDI", 0, 40);
            ReadData(reader, data, "DogHP", baseOffset, 798, levelBlockSize, "WORDI", 0, 16);
            ReadData(reader, data, "AhmetHP", baseOffset, 800, levelBlockSize, "WORDI", 0, 80);
            ReadData(reader, data, "BaboonHP", baseOffset, 802, levelBlockSize, "WORDI", 0, 30);
            ReadData(reader, data, "BatHP", baseOffset, 804, levelBlockSize, "WORDI", 0, 5);
            ReadData(reader, data, "BigBeetleHP", baseOffset, 806, levelBlockSize, "WORDI", 0, 30);
            ReadData(reader, data, "VonCroyHP", baseOffset, 808, levelBlockSize, "WORDI", 0, 15);

            // Enemy Damage
            ReadData(reader, data, "BaddyUZIDmg", baseOffset, 810, levelBlockSize, "BYTE", 0, 15);
            ReadData(reader, data, "SASMachinegunDmg", baseOffset, 811, levelBlockSize, "BYTE", 0, 15);
            ReadData(reader, data, "TurretDmg", baseOffset, 812, levelBlockSize, "BYTE", 0, 5);
            ReadData(reader, data, "BatDmg", baseOffset, 813, levelBlockSize, "BYTEI", 0, 2);
            ReadData(reader, data, "CrocUWDmg", baseOffset, 814, levelBlockSize, "BYTEI", 0, 120);
            ReadData(reader, data, "CrocLandDmg", baseOffset, 815, levelBlockSize, "BYTEI", 0, 120);
            ReadData(reader, data, "LocustDmg", baseOffset, 816, levelBlockSize, "BYTEI", 0, 3);
            ReadData(reader, data, "MummyDmg", baseOffset, 817, levelBlockSize, "BYTEI", 0, 100);
            ReadData(reader, data, "BaddySwordDmg", baseOffset, 818, levelBlockSize, "BYTEI", 0, 120);
            ReadData(reader, data, "SmallScorpionDmg", baseOffset, 819, levelBlockSize, "BYTEI", 0, 20);
            ReadData(reader, data, "DogDmg", baseOffset, 820, levelBlockSize, "BYTEI", 0, 10);
            ReadData(reader, data, "SkeletonAttack1Dmg", baseOffset, 821, levelBlockSize, "BYTEI", 0, 80);
            ReadData(reader, data, "SkeletonAttack2Dmg", baseOffset, 822, levelBlockSize, "BYTEI", 0, 80);
            ReadData(reader, data, "WildBoarDmg", baseOffset, 823, levelBlockSize, "BYTEI", 0, 30);
            ReadData(reader, data, "HarpyDmg", baseOffset, 824, levelBlockSize, "BYTEI", 0, 10);
            ReadData(reader, data, "ScorpionDmg", baseOffset, 825, levelBlockSize, "BYTEI", 0, 120);
            ReadData(reader, data, "HammerheadDmg", baseOffset, 826, levelBlockSize, "BYTEI", 0, 120);
            ReadData(reader, data, "KnightTemplarDmg", baseOffset, 827, levelBlockSize, "BYTEI", 0, 120);
            ReadData(reader, data, "BigBeetleDmg", baseOffset, 828, levelBlockSize, "BYTEI", 0, 50);
            ReadData(reader, data, "SphinxDmg", baseOffset, 829, levelBlockSize, "WORDI", 0, 200);
            ReadData(reader, data, "SethAttack1Dmg", baseOffset, 831, levelBlockSize, "WORDI", 0, 200);
            ReadData(reader, data, "SethAttack2Dmg", baseOffset, 833, levelBlockSize, "WORDI", 0, 250);

            // Ponytail / Motorbike
            ReadData(reader, data, "PonytailMode", baseOffset, 835, levelBlockSize, "BYTE", 0, 0);
            ReadData(reader, data, "MotorbikeHeadlight", baseOffset, 836, levelBlockSize, "BOOL", 0, true);

            // MP Bar
            ReadData(reader, data, "MPBarRGB1", baseOffset, 837, levelBlockSize, "HEX", 0, "#ffaa00");
            ReadData(reader, data, "MPBarRGB2", baseOffset, 840, levelBlockSize, "HEX", 0, "#000000");
            ReadData(reader, data, "MPBarDecSpeed", baseOffset, 843, levelBlockSize, "BYTE", 0, 30);
            ReadData(reader, data, "MPBarIncSpeed", baseOffset, 844, levelBlockSize, "BYTE", 0, 15);
            ReadData(reader, data, "MPBarHPDecSpeed", baseOffset, 845, levelBlockSize, "BYTE", 0, 40);
            ReadData(reader, data, "AudioPath", baseOffset, 847, levelBlockSize, "FIXED_STRING", 53, @"audio\%s");
            ReadData(reader, data, "HardClipRange", baseOffset, 900, levelBlockSize, "DWORD", 0, 20480);
            ReadData(reader, data, "LoadingPath", baseOffset, 904, levelBlockSize, "FIXED_STRING", 40,
                @"screens\loading.bmp");
            ReadData(reader, data, "OptionsScreen", baseOffset, 944, levelBlockSize, "FIXED_STRING", 40,
                @"screens\options.bmp");
            ReadData(reader, data, "InventoryScreen", baseOffset, 984, levelBlockSize, "FIXED_STRING", 40,
                @"screens\inventory.bmp");
            ReadData(reader, data, "LoadSaveScreen", baseOffset, 1024, levelBlockSize, "FIXED_STRING", 40,
                @"screens\loadsave.bmp");

            // Names
            ReadData(reader, data, "SkeletonName", baseOffset, 1064, levelBlockSize, "FIXED_STRING", 40, "Skeleton");
            ReadData(reader, data, "GuideName", baseOffset, 1104, levelBlockSize, "FIXED_STRING", 40, "Guide");
            ReadData(reader, data, "VonCroynName", baseOffset, 1144, levelBlockSize, "FIXED_STRING", 40, "Von Croy");
            ReadData(reader, data, "Baddy1Name", baseOffset, 1184, levelBlockSize, "FIXED_STRING", 40, "Baddy");
            ReadData(reader, data, "Baddy2Name", baseOffset, 1224, levelBlockSize, "FIXED_STRING", 40, "Black Baddy");
            ReadData(reader, data, "SethaName", baseOffset, 1264, levelBlockSize, "FIXED_STRING", 40, "Seth");
            ReadData(reader, data, "MummyName", baseOffset, 1304, levelBlockSize, "FIXED_STRING", 40, "Mummy");
            ReadData(reader, data, "SphinxName", baseOffset, 1344, levelBlockSize, "FIXED_STRING", 40, "Sphinx");
            ReadData(reader, data, "CrocodileName", baseOffset, 1384, levelBlockSize, "FIXED_STRING", 40, "Crocodile");
            ReadData(reader, data, "HorsemanName", baseOffset, 1424, levelBlockSize, "FIXED_STRING", 40, "Horseman");
            ReadData(reader, data, "ScorpionName", baseOffset, 1464, levelBlockSize, "FIXED_STRING", 40, "Scorpion");
            ReadData(reader, data, "JeanYvesName", baseOffset, 1504, levelBlockSize, "FIXED_STRING", 40, "Jean Yves");
            ReadData(reader, data, "TroopsName", baseOffset, 1544, levelBlockSize, "FIXED_STRING", 40, "Troops");
            ReadData(reader, data, "KnightTemplarName", baseOffset, 1584, levelBlockSize, "FIXED_STRING", 40,
                "Knight Templar");
            ReadData(reader, data, "MutantName", baseOffset, 1624, levelBlockSize, "FIXED_STRING", 40, "Mutant");
            ReadData(reader, data, "HorseName", baseOffset, 1664, levelBlockSize, "FIXED_STRING", 40, "Horse");
            ReadData(reader, data, "BaboonNormalName", baseOffset, 1704, levelBlockSize, "FIXED_STRING", 40, "Baboon");
            ReadData(reader, data, "BaboonInvName", baseOffset, 1744, levelBlockSize, "FIXED_STRING", 40,
                "Baboon (invisible)");
            ReadData(reader, data, "BaboonSilentName", baseOffset, 1784, levelBlockSize, "FIXED_STRING", 40,
                "Baboon (silent)");
            ReadData(reader, data, "WildBoarName", baseOffset, 1824, levelBlockSize, "FIXED_STRING", 40, "Wild boar");
            ReadData(reader, data, "HarpyName", baseOffset, 1864, levelBlockSize, "FIXED_STRING", 40, "Harpy");
            ReadData(reader, data, "Demigod1Name", baseOffset, 1904, levelBlockSize, "FIXED_STRING", 40, "Demigod");
            ReadData(reader, data, "Demigod2Name", baseOffset, 1944, levelBlockSize, "FIXED_STRING", 40, "Demigod 2");
            ReadData(reader, data, "Demigod3Name", baseOffset, 1984, levelBlockSize, "FIXED_STRING", 40, "Demigod 3");
            ReadData(reader, data, "BigBeetleName", baseOffset, 2024, levelBlockSize, "FIXED_STRING", 40, "Beetle");
            ReadData(reader, data, "BatName", baseOffset, 2064, levelBlockSize, "FIXED_STRING", 40, "Bat");
            ReadData(reader, data, "DogName", baseOffset, 2104, levelBlockSize, "FIXED_STRING", 40, "Dog");
            ReadData(reader, data, "HammerheadName", baseOffset, 2144, levelBlockSize, "FIXED_STRING", 40,
                "Hammer-headed shark");
            ReadData(reader, data, "SASName", baseOffset, 2184, levelBlockSize, "FIXED_STRING", 40,
                "Special Air Service soldier");
            ReadData(reader, data, "AhmetName", baseOffset, 2224, levelBlockSize, "FIXED_STRING", 40, "Ahmet");
            ReadData(reader, data, "LaraDoubleName", baseOffset, 2264, levelBlockSize, "FIXED_STRING", 40,
                "Strange statue...");
            ReadData(reader, data, "SmallScorpionName", baseOffset, 2304, levelBlockSize, "FIXED_STRING", 40,
                "Small scorpion");
            ReadData(reader, data, "SentryGunName", baseOffset, 2344, levelBlockSize, "FIXED_STRING", 40,
                "SAS Sentry Gun");

            // Horizontal Mirrors
            var horMirrorOffset = 2384;
            for (var j = 1; j < 21; j++)
            {
                var horMirrorKey = "HorMirror" + j.ToString().PadLeft(2, '0');
                var horMirrorRoomKey = $"{horMirrorKey}Room";
                ReadData(reader, data, horMirrorKey, baseOffset, horMirrorOffset, levelBlockSize, "BYTE", 0, 255);
                horMirrorOffset++;
                ReadData(reader, data, horMirrorRoomKey, baseOffset, horMirrorOffset, levelBlockSize, "DWORD", 0, 0);
                horMirrorOffset += 4;
            }

            // Vertical Mirrors
            var vertMirrorOffset = 2484;
            for (var j = 1; j < 51; j++)
            {
                var vertMirrorKey = "VertMirror" + j.ToString().PadLeft(2, '0');
                var vertMirrorLimKey = $"{vertMirrorKey}Lim";
                var vertMirrorRoomKey = $"{vertMirrorKey}Room";
                ReadData(reader, data, vertMirrorKey, baseOffset, vertMirrorOffset, levelBlockSize, "BYTE", 0, 255);
                vertMirrorOffset++;
                ReadData(reader, data, vertMirrorLimKey, baseOffset, vertMirrorOffset, levelBlockSize, "BOOL", 0,
                    false);
                vertMirrorOffset++;
                ReadData(reader, data, vertMirrorRoomKey, baseOffset, vertMirrorOffset, levelBlockSize, "DWORD", 0, 0);
                vertMirrorOffset += 4;
            }

            ReadData(reader, data, "ColdBreath", baseOffset, 2784, levelBlockSize, "BOOL", 0, true);
            ReadData(reader, data, "SlowStartupFade", baseOffset, 2785, levelBlockSize, "BOOL", 0, false);
            ReadData(reader, data, "DisableSave", baseOffset, 2790, levelBlockSize, "BOOL", 0, false);

            var gfxInfo = new GfxInfo();
            var levelInfo = new LevelData();

            if (data.TryGetValue("ColdBreath", out var coldBreathVal) && (bool)coldBreathVal)
                gfxInfo.ColdBreath = "enabled_outside_and_in_cold_rooms";

            if (patchData.MetaInfo?.EsseScriptedParams ?? false)
            {
                var environmentInfo = new EnvironmentInfo();
                var objectsInfo = new ObjectsInfo();

                // Darts
                var dartsInterval = data.GetValueOrDefault("DartsInterval", EsseConstants.DartsIntervalDefault);
                objectsInfo.DartsInterval = ((short?)dartsInterval).NullIf(EsseConstants.DartsIntervalDefault);
                var dartsSpeed = data.GetValueOrDefault("DartsSpeed", EsseConstants.DartsSpeedDefault)
                    .NullIf(EsseConstants.DartsSpeedDefault);
                objectsInfo.DartsSpeed = (short?)dartsSpeed;
                if ((string?)data!.GetValueOrDefault("DartsRGB", null) != EsseConstants.DartsRgb)
                    _logger.LogInformation("Darts color is not default");

                // Falling block
                objectsInfo.FallingBlockTimer = (short?)data
                    .GetValueOrDefault("FallingBlockTimeout", EsseConstants.FallingBlockTimerDefault)
                    .NullIf(EsseConstants.FallingBlockTimerDefault);

                objectsInfo.FallingBlockTremble = (short?)data
                    .GetValueOrDefault("FallingBlockTremble", EsseConstants.FallingBlockTrembleDefault)
                    .NullIf(EsseConstants.FallingBlockTrembleDefault);

                objectsInfo.ObjectCustomization = [];
                for (var j = 0; j < Constants.ObjectCount; j++)
                {
                    objectsInfo.ObjectCustomization.Add(new ObjectCustomization());
                }

                environmentInfo.FogStartRange = data.TryGetValue("DF", out var df) ? (int)(float)df : null;
                environmentInfo.FogEndRange = data.TryGetValue("DFThresh", out var dfThresh) ? (int)(float)dfThresh : null;
                environmentInfo.FarView = data.TryGetValue("DD", out var dd) ? (int)(float)dd : null;

                foreach (var row in Tomb4DataTables.EnemyHealthTable)
                {
                    var enemyName = row.Name;
                    var enemyHp = (int?)data.GetValueOrDefault(enemyName);
                    if (enemyHp == null)
                        continue;

                    var slotNumber = row.SlotNumber;
                    objectsInfo.ObjectCustomization[slotNumber].HitPoints = enemyHp;
                }

                levelInfo.EnvironmentInfo = environmentInfo;
                levelInfo.ObjectsInfo = objectsInfo;
            }
            
            if (patchData.MetaInfo?.EsseMultipleMirrors ?? false)
            {
                var mirrorCustomizations = new List<MirrorCustomization>();
                for (var j = 1; j < 21; j++)
                {
                    var horMirrorKey = "HorMirror" + j.ToString().PadLeft(2, '0');
                    var horMirrorRoomKey = $"{horMirrorKey}Room";
                    var roomNumber = (int)(data.GetValueOrDefault(horMirrorKey) ?? 255);
                    if (roomNumber != 255)
                    {
                        var planePosition = (int)data[horMirrorRoomKey];
                        mirrorCustomizations.Add(
                            new MirrorCustomization()
                            {
                                RoomNumber = roomNumber, 
                                PlanePosition = planePosition, 
                                PlaneDirection = "z"
                            });
                    }
                }

                for (var j = 1; j < 51; j++)
                {
                    var vertMirrorKey = "VertMirror" + j.ToString().PadLeft(2, '0');
                    var vertMirrorRoomKey = $"{vertMirrorKey}Room";
                    var roomNumber = (int)(data.GetValueOrDefault(vertMirrorKey) ?? 255);
                    if (roomNumber != 255)
                    {
                        var planePosition = (int)data[vertMirrorRoomKey];
                        mirrorCustomizations.Add(
                            new MirrorCustomization()
                            {
                                RoomNumber = roomNumber, 
                                PlanePosition = planePosition, 
                                PlaneDirection = "y"
                            });
                    }
                }

                gfxInfo.MirrorCustomization = mirrorCustomizations;
            }

            levelInfo.GfxInfo = gfxInfo;
            levelArray.Add(levelInfo);
        }


        return levelArray;
    }
}