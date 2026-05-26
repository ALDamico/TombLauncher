namespace TombLauncher.Patchers.Tomb4Plus.Models;

public record EnemyHealthEntry(long Address, string Name, short Default, int SlotNumber);
public record EnemyDamageEntry(long Address, string Name, int Default, int Size,
    bool InvertSign, int[] SlotNumbers, int DamageId);

public static class Tomb4DataTables
{
    public static IReadOnlyList<EnemyHealthEntry> EnemyHealthTable { get; } =
    [
        new(0x0005B770, "SkeletonHP",      15,     35),
        new(0x0005BB30, "Baddy1HP",        25,     41),
        new(0x0005BD1D, "Baddy2HP",        35,     43),
        new(0x0005BEFF, "ScorpionHP",      80,     55),
        new(0x0005BFE9, "MummyHP",         15,     47),
        new(0x0005C087, "KnightTemplarHP", 15,     61),
        new(0x0005C105, "SphinxHP",        1000,   49),
        new(0x0005C167, "SethHP",          500,    45),
        new(0x0005C21C, "HorsemenHP",      25,     53),
        new(0x0005C28D, "HammerheadHP",    45,     93),
        new(0x0005C33D, "CrocHP",          36,     51),
        new(0x0005C5FE, "MutantHP",        15,     63),
        new(0x0005B99A, "GuideHP",         -16384, 37),
        new(0x0005C3F7, "Demigod1HP",      200,    77),
        new(0x0005C4A3, "Demigod2HP",      200,    79),
        new(0x0005C54F, "Demigod3HP",      200,    81),
        new(0x0005C69E, "TroopsHP",        40,     59),
        new(0x0005C74E, "SASHP",           40,     95),
        new(0x0005C7E9, "HarpyHP",         60,     75),
        new(0x0005C86C, "WildBoarHP",      40,     73),
        new(0x0005C911, "DogHP",           16,     91),
        new(0x0005C9A4, "AhmetHP",         80,     102),
        new(0x0005CA03, "BaboonHP",        30,     67),
        new(0x0005CB54, "BatHP",           5,      90),
        new(0x0005CBB9, "BigBeetleHP",     30,     84),
        new(0x0005B804, "VonCroyHP",       15,     39),
    ];

    public static IReadOnlyList<EnemyDamageEntry> EnemyDamageTable { get; } =
    [
        new(0x0000C9FE, "baddy_uzi",            15,  1, false, [41, 43], 2),
        new(0x0000D803, "sas_machinegun",        15,  1, false, [95],     1),
        new(0x0003F200, "turret",                5,   1, false, [162],    1),
        new(0x00002CD5, "bat",                   2,   1, true,  [90],     2),
        new(0x000033C0, "crocodile_land",        120, 1, true,  [51],     1),
        new(0x00003BAF, "locust",                3,   1, true,  [107],    1),
        new(0x0000AD3E, "mummy",                 100, 1, true,  [47],     1),
        new(0x000031C8, "baddy_sword_a",         120, 1, true,  [41, 43], 1),
        new(0x0000C4CC, "baddy_sword_b",         120, 1, true,  [41, 43], 1),
        new(0x0000F18C, "small_scorpion",        20,  1, true,  [106],    1),
        new(0x00005DA6, "dog",                   10,  1, true,  [91],     1),
        new(0x00012368, "skeleton_attack_1",     80,  1, true,  [35],     1),
        new(0x0001257E, "skeleton_attack_2",     80,  1, true,  [35],     2),
        new(0x0001C0C6, "wild_boar",             30,  1, true,  [73],     1),
        new(0x00007E44, "harpy",                 10,  1, true,  [75],     1),
        new(0x0000EDA8, "scorpion",              120, 1, true,  [55],     1),
        new(0x0000718F, "hammerhead",            120, 1, true,  [93],     1),
        new(0x000133B1, "knights_templar",       120, 1, true,  [61],     1),
        new(0x0000E10B, "big_beetle",            50,  1, true,  [84],     1),
        new(0x00012D2E, "sphinx",                200, 2, true,  [49],     1),
        new(0x000110BC, "set_attack_1",          200, 2, true,  [45],     1),
        new(0x00011285, "set_attack_2",          250, 2, true,  [45],     2),
    ];
}