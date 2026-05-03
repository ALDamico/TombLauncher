using TombLauncher.Core.Extensions;
using TombLauncher.Patchers.OriginalEngines.Models;
using TombLauncher.Patchers.Shared.Models;

namespace TombLauncher.Patchers.Shared;

public class GameflowMapper
{
    public GameflowDto ToDto(Gameflow gameflow)
    {
        var levelSequences = gameflow.Sequences.SplitAt(s => s.Opcode == SequenceOpcode.End);
        var dto = new GameflowDto()
        {
            DemoTimeout = gameflow.DemoTime.FromGameTicks(),
            Description = gameflow.Description,
            FirstOption = gameflow.FirstOption,
            Flags = gameflow.Flags,
            GameflowSize = gameflow.GameflowSize,
            IsSingleLevel = gameflow.SingleLevel != ushort.MaxValue,
            LanguageId = gameflow.LanguageId,
            NumChapterScreens = gameflow.NumChapterScreens,
            NumCutscenes = gameflow.NumCutscenes,
            NumDemoLevels = gameflow.NumDemoLevels,
            NumFmvs = gameflow.NumFmvs,
            NumLevels = gameflow.NumLevels,
            NumTitles = gameflow.NumTitles,
            OnDeathDemoMode = gameflow.OnDeathDemoMode,
            OnDeathInGame = gameflow.OnDeathInGame,
            OnDemoEnd = gameflow.OnDemoEnd,
            OnDemoInterrupt = gameflow.OnDemoInterrupt,
            SecretSoundId = gameflow.SecretSoundId,
            TitleReplace = gameflow.TitleReplace,
            TitleSoundId = gameflow.TitleSoundId,
            Version = gameflow.Version,
            XorKey = gameflow.XorKey
        };

        for (var i = 0; i < gameflow.NumLevels; i++)
        {
            dto.Levels.Add(ConvertLevel(gameflow, i, levelSequences));
        }

        return dto;
    }

    private SequenceInfo ConvertSequence(Sequence sequence)
    {
        var sequenceInfo = new SequenceInfo()
        {
            Opcode = sequence.Opcode,
            Argument = sequence.Argument
        };

        switch (sequence.Opcode)
        {
            case SequenceOpcode.Fmv:
                sequenceInfo.FmvId = sequence.Argument;
                break;
            case SequenceOpcode.Level:
            case SequenceOpcode.Demo:
                sequenceInfo.LevelId = sequence.Argument;
                break;
            case SequenceOpcode.Cine:
                sequenceInfo.CutsceneId = sequence.Argument;
                break;
            case SequenceOpcode.JumpToSequence:
                sequenceInfo.SequenceId = sequence.Argument;
                break;
            case SequenceOpcode.Track:
                sequenceInfo.TrackId = sequence.Argument;
                break;
            case SequenceOpcode.LoadPic:
            case SequenceOpcode.Picture:
                sequenceInfo.PictureId = sequence.Argument;
                break;
            case SequenceOpcode.CutAngle:
                sequenceInfo.RotationAngle = sequence.Argument.FromBam();
                break;
            case SequenceOpcode.NoFloor:
                sequenceInfo.Depth = sequence.Argument;
                break;
            case SequenceOpcode.StartInv:
                sequenceInfo.InventoryItemId = sequence.Argument;
                break;
            case SequenceOpcode.StartAnim:
                sequenceInfo.AnimationId = sequence.Argument;
                break;
            case SequenceOpcode.Secrets:
                sequenceInfo.CountsForSecrets = sequence.Argument != 0;
                break;
        }

        return sequenceInfo;
    }

    private LevelInfo ConvertLevel(Gameflow gameflow, int index, List<List<Sequence>> levelSequences)
    {
        var levelInfo = new LevelInfo()
        {
            FilePath = ApplyXorIfRequired(gameflow.LevelPathStrings![index], gameflow).GetNullTerminatedString(),
            Title = ApplyXorIfRequired(gameflow.LevelStrings![index], gameflow).GetNullTerminatedString(),
            Sequence = levelSequences[index].Select(ConvertSequence).ToList()
        };

        foreach (var puzzleItem in gameflow.PuzzleStrings)
        {
            var puzzleItemName = ApplyXorIfRequired(puzzleItem[index], gameflow).GetNullTerminatedString();
            levelInfo.PuzzleItemNames.Add(puzzleItemName);
        }

        foreach (var pickupItem in gameflow.PickupStrings)
        {
            var pickupItemName = ApplyXorIfRequired(pickupItem[index], gameflow).GetNullTerminatedString();
            levelInfo.PickupItemNames.Add(pickupItemName);
        }

        foreach (var keyItem in gameflow.KeyStrings)
        {
            var keyItemName = ApplyXorIfRequired(keyItem[index], gameflow).GetNullTerminatedString();
            levelInfo.KeyNames.Add(keyItemName);
        }

        return levelInfo;
    }

    private byte[] ApplyXorIfRequired(byte[] arr, Gameflow gameflow)
    {
        if (gameflow.Flags.HasFlag(GameflowFlags.UseXor) && gameflow.XorKey != 0)
        {
            return arr.Select(b => (byte)(b ^ gameflow.XorKey)).ToArray();
        }

        return arr;
    }
}