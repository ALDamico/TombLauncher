using System;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Utils;
using TombLauncher.Data.Database.Services;
using TombLauncher.Factories.Mapping;
using TombLauncher.Localization.Extensions;
using TombLauncher.ValueConverters;
using TombLauncher.ViewModels;

namespace TombLauncher.Services;

public class StatisticsService
{
    public StatisticsService(
        ISettingsProvider settingsProvider,
        StatisticsDataService statisticsDataService,
        IPlatformSpecificFeatures platformSpecificFeatures,
        StatisticsMapper mapper)
    {
        _settingsProvider = settingsProvider;
        _statisticsDataService = statisticsDataService;
        _platformSpecificFeatures = platformSpecificFeatures;
        _mapper = mapper;
    }

    private readonly ISettingsProvider _settingsProvider;
    private readonly StatisticsDataService _statisticsDataService;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;
    private readonly StatisticsMapper _mapper;

    public long GetDatabaseSize()
    {
        var databasePath = _settingsProvider.GetApplicationSettings().DatabasePath;
        databasePath = Path.Combine(_platformSpecificFeatures.GetAppDataDirectory(), databasePath);
        var fileInfo = new FileInfo(databasePath);
        return fileInfo.Length;
    }

    public long GetGamesSize()
    {
        var gamesFolder = PathUtils.GetGamesFolder(_platformSpecificFeatures.GetAppDataDirectory());
        return PathUtils.GetDirectorySize(gamesFolder);
    }

    public async Task<StatisticsViewModel> GetStatistics()
    {
        var statistics = await _statisticsDataService.GetStatistics();
        return new StatisticsViewModel()
        {
            MostLaunches = _mapper.ToViewModel(statistics.MostLaunches),
            LatestPlayedGame = _mapper.ToViewModel(statistics.LatestPlayedGame),
            LongestPlaySession = _mapper.ToViewModel(statistics.LongestPlaySession),

            DayOfWeekAveragePlayTimeStatistics = new ChartViewModel()
            {
                Series =
                [
                    new LineSeries<DayOfWeekAverageTimeStatisticsViewModel>()
                    {
                        Values = _mapper.ToDayOfWeekAverageTimeStatisticsViewModels(statistics.DayOfWeekStatistics),
                        ScalesYAt = 0,
                    }
                ],
                XAxis =
                [
                    new Axis()
                    {
                        Name = "DAY_OF_WEEK".GetLocalizedString(),
                        Labels = DateTimeExtensions.GetAbbreviatedDayNamesOrdered(CultureInfo.CurrentUICulture),
                    }
                ],
                YAxis =
                [
                    new TimeSpanAxis(
                        TimeSpan.FromMinutes(15),
                        v => string.Format("{0:%h} h {0:%m} min", v))
                    {
                        Name = "AVERAGE_TIME_PLAYED".GetLocalizedString(),
                    }
                ]
            },
            DayOfWeekTotalGamesPlayedStatistics = new ChartViewModel()
            {
                Series = new ISeries[]
                {
                    new ColumnSeries<DayOfWeekPlaySessionCountStatisticsViewModel>()
                    {
                        Values = statistics.DayOfWeekStatistics.Select(s =>
                            new DayOfWeekPlaySessionCountStatisticsViewModel()
                            {
                                PlayCount = s.PlaySessionsCount, DayOfWeek = s.DayOfWeek,
                                Index = s.DayOfWeek.GetCultureSensitiveOrder(CultureInfo.CurrentUICulture)
                            }).ToList(),
                        ScalesYAt = 0
                    }
                },
                XAxis = new[]
                {
                    new Axis()
                    {
                        Name = "DAY_OF_WEEK".GetLocalizedString(),
                        Labels = DateTimeExtensions.GetAbbreviatedDayNamesOrdered(CultureInfo.CurrentUICulture)
                    }
                },
                YAxis = new[]
                {
                    new Axis()
                    {
                        MinStep = 1,
                        Labeler = v =>
                            ((int)v).ToString() + " " + "GAMES_PLAYED".GetLocalizedString(),
                        Name = "TOTAL_GAMES_PLAYED".GetLocalizedString(),
                    }
                }
            },
            DailyAverageGameLengthStatistics = new ChartViewModel()
            {
                Series =
                [
                    new LineSeries<DailyGameDurationViewModel>()
                    {
                        Values = _mapper.ToViewModels(statistics.DailyStatistics, false),
                        ScalesYAt = 0,
                        Name = "AVERAGE_PLAY_TIME".GetLocalizedString()
                    }, new LineSeries<DailyGameDurationViewModel>()
                    {
                        Values = _mapper.ToViewModels(statistics.DailyStatistics, true),
                        ScalesYAt = 0,
                        Name = "TOTAL_PLAY_TIME".GetLocalizedString()
                    }
                ],
                XAxis =
                [
                    new DateTimeAxis(TimeSpan.FromDays(1), v => v.ToString("d"))
                    {
                        Name = "DAY".GetLocalizedString()
                    }
                ],
                YAxis =
                [
                    new TimeSpanAxis(TimeSpan.FromMinutes(15), v => string.Format("{0:%h} h {0:%m} min" , v))
                    {
                        Name = "TIME_PLAYED".GetLocalizedString(),
                    }
                ]
            },
            SpaceUsedStatistics = new ChartViewModel()
            {
                Series = statistics.SpaceUsedStatistics.AsPieSeries((_, series) =>
                {
                    series.ToolTipLabelFormatter = point => point.Model?.Title + Environment.NewLine + (string?)new FileSizeConverter().Convert(
                        point.Model?.SpaceUsedBytes ?? 0, typeof(string), null!,
                        CultureInfo.InvariantCulture);
                    LiveCharts.Configure(config => config
                        .HasMap<GameSpaceUsedDto>((point, index) => new(index, point.SpaceUsedBytes)));
                }).ToArray<ISeries>()
            }
        };
    }
}