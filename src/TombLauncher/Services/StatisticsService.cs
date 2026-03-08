using System;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoMapper;
using CommunityToolkit.Mvvm.DependencyInjection;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Utils;
using TombLauncher.Data.Database.Services;
using TombLauncher.Localization.Extensions;
using TombLauncher.ValueConverters;
using TombLauncher.ViewModels;

namespace TombLauncher.Services;

public class StatisticsService
{
    public StatisticsService(
        ISettingsProvider settingsProvider,
        StatisticsDataService statisticsDataService,
        MapperConfiguration mapperConfiguration,
        IPlatformSpecificFeatures platformSpecificFeatures)
    {
        _settingsProvider = settingsProvider;
        _statisticsDataService = statisticsDataService;
        _mapper = mapperConfiguration.CreateMapper();
        _platformSpecificFeatures = platformSpecificFeatures;
    }

    private readonly ISettingsProvider _settingsProvider;
    private readonly StatisticsDataService _statisticsDataService;
    private readonly IMapper _mapper;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;

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
            MostLaunches = _mapper.Map<GameStatisticsViewModel>(statistics.MostLaunches),
            LatestPlayedGame = _mapper.Map<GameStatisticsViewModel>(statistics.LatestPlayedGame),
            LongestPlaySession = _mapper.Map<GameStatisticsViewModel>(statistics.LongestPlaySession),

            DayOfWeekAveragePlayTimeStatistics = new ChartViewModel()
            {
                Series = new ISeries[]
                {
                    new LineSeries<DayOfWeekAverageTimeStatisticsViewModel>()
                    {
                        Values = statistics.DayOfWeekStatistics.Select(s =>
                            new DayOfWeekAverageTimeStatisticsViewModel()
                            {
                                AverageTimePlayed = s.AverageTimePlayed,
                                DayOfWeek = s.DayOfWeek,
                                Index = s.DayOfWeek.GetCultureSensitiveOrder(CultureInfo.CurrentUICulture)
                            }).ToList(),
                        ScalesYAt = 0,
                    }
                },
                XAxis = new[]
                {
                    new Axis()
                    {
                        Name = "DAY_OF_WEEK".GetLocalizedString(),
                        Labels = DateTimeExtensions.GetAbbreviatedDayNamesOrdered(CultureInfo.CurrentUICulture),
                    }
                },
                YAxis = new[]
                {
                    new TimeSpanAxis(
                        TimeSpan.FromMinutes(15),
                        v => string.Format("{0:%h} h {0:%m} min", v))
                    {
                        Name = "AVERAGE_TIME_PLAYED".GetLocalizedString(),
                    }
                }
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
                        Labeler = v =>
                            v.ToString(CultureInfo.InvariantCulture) + " " + "GAMES_PLAYED".GetLocalizedString(),
                        Name = "TOTAL_GAMES_PLAYED".GetLocalizedString(),
                    }
                }
            },
            DailyAverageGameLengthStatistics = new ChartViewModel()
            {
                Series = new[]
                {
                    new LineSeries<DailyStatisticsAverageGameDurationViewModel>()
                    {
                        Values = statistics.DailyStatistics.Select(s =>
                            new DailyStatisticsAverageGameDurationViewModel()
                                { Date = s.Date, AverageGameDuration = s.AverageGameDuration }).ToList(),
                        ScalesYAt = 0,
                        Name = "AVERAGE_PLAY_TIME".GetLocalizedString()
                    }, new LineSeries<DailyStatisticsAverageGameDurationViewModel>()
                    {
                        Values = statistics.DailyStatistics.Select(s =>
                            new DailyStatisticsAverageGameDurationViewModel()
                                { Date = s.Date, AverageGameDuration = s.TotalPlayTime }).ToList(),
                        ScalesYAt = 0,
                        Name = "TOTAL_PLAY_TIME".GetLocalizedString()
                    }
                },
                XAxis = new[]
                {
                    new DateTimeAxis(TimeSpan.FromDays(1), v => v.ToString("d"))
                    {
                        Name = "DAY".GetLocalizedString()
                    }
                },
                YAxis = new[]
                {
                    new TimeSpanAxis(TimeSpan.FromMinutes(15), v => string.Format("{0:%h} h {0:%m} min" , v))
                    {
                        Name = "TIME_PLAYED".GetLocalizedString(),
                    }
                }
            },
            SpaceUsedStatistics = new ChartViewModel()
            {
                Series = statistics.SpaceUsedStatistics.AsPieSeries((dto, series) =>
                {
                    series.ToolTipLabelFormatter = point => point.Model?.Title + Environment.NewLine + (string?)new FileSizeConverter().Convert(
                        point.Model?.SpaceUsedBytes ?? 0, typeof(string), null!,
                        CultureInfo.InvariantCulture);
                    LiveCharts.Configure(config => config
                        .HasMap<GameSpaceUsedDto>((point, index) => new(index, point.SpaceUsedBytes)));
                }).ToArray()
            }
        };
    }
}