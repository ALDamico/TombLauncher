using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.ViewModels;

namespace TombLauncher.Factories.Mapping;

public class StatisticsMapper
{
    public DayOfWeekAverageTimeStatisticsViewModel ToDayOfWeekAverageTimeStatisticsViewModel(DayOfWeekStatisticsDto dto)
    {
        return new DayOfWeekAverageTimeStatisticsViewModel()
        {
            AverageTimePlayed = dto.AverageTimePlayed,
            DayOfWeek = dto.DayOfWeek,
            Index = dto.DayOfWeek.GetCultureSensitiveOrder(CultureInfo.CurrentUICulture)
        };
    }

    public List<DayOfWeekAverageTimeStatisticsViewModel>
        ToDayOfWeekAverageTimeStatisticsViewModels(IEnumerable<DayOfWeekStatisticsDto> dtos) =>
        dtos.Select(ToDayOfWeekAverageTimeStatisticsViewModel).ToList();

    public GameStatisticsViewModel? ToViewModel(GameStatisticsDto? dto)
    {
        if (dto == null)
            return null;

        return new GameStatisticsViewModel()
        {
            Id = dto.Id,
            LastPlayed = dto.LastPlayed,
            LastPlayedEnd = dto.LastPlayedEnd,
            Title = dto.Title,
            TotalSessions = dto.TotalSessions
        };
    }

    public DailyGameDurationViewModel ToViewModel(DailyStatisticsDto dto, bool isTotal)
    {
        var vm = new DailyGameDurationViewModel()
        {
            Date = dto.Date
        };
        vm.GameDuration = isTotal ? dto.TotalPlayTime : dto.AverageGameDuration;

        return vm;
    }

    public List<DailyGameDurationViewModel> ToViewModels(IEnumerable<DailyStatisticsDto> dto, bool isTotal) =>
        dto.Select(d => ToViewModel(d, isTotal)).ToList();
}