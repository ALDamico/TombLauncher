﻿using System;
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
using TombLauncher.Core.Utils;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Localization.Extensions;
using TombLauncher.ValueConverters;
using TombLauncher.ViewModels;

namespace TombLauncher.Services;

public class StatisticsService
{
    public StatisticsService()
    {
        _settingsService = Ioc.Default.GetRequiredService<SettingsService>();
        _gamesUnitOfWork = Ioc.Default.GetRequiredService<GamesUnitOfWork>();
        _mapper = Ioc.Default.GetRequiredService<MapperConfiguration>().CreateMapper();
    }

    private readonly SettingsService _settingsService;
    private readonly GamesUnitOfWork _gamesUnitOfWork;
    private readonly IMapper _mapper;

    public long GetDatabaseSize()
    {
        var databasePath = _settingsService.GetDatabasePath();
        var fileInfo = new FileInfo(databasePath);
        return fileInfo.Length;
    }

    public long GetGamesSize()
    {
        var gamesFolder = PathUtils.GetGamesFolder();
        return PathUtils.GetDirectorySize(gamesFolder);
    }

    public StatisticsViewModel GetStatistics()
    {
        var statistics = _gamesUnitOfWork.GetStatistics();
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
                        Name = "Day of week".GetLocalizedString(),
                        Labels = DateTimeExtensions.GetAbbreviatedDayNamesOrdered(CultureInfo.CurrentUICulture),
                    }
                },
                YAxis = new[]
                {
                    new TimeSpanAxis(
                        TimeSpan.FromMinutes(15),
                        v => string.Format("{0:%h} h {0:%m} min", v))
                    {
                        Name = "Average time played".GetLocalizedString(),
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
                        Name = "Day of week".GetLocalizedString(),
                        Labels = DateTimeExtensions.GetAbbreviatedDayNamesOrdered(CultureInfo.CurrentUICulture)
                    }
                },
                YAxis = new[]
                {
                    new Axis()
                    {
                        Labeler = v =>
                            v.ToString(CultureInfo.InvariantCulture) + " " + "games played".GetLocalizedString(),
                        Name = "Total games played".GetLocalizedString(),
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
                        Name = "Average play time".GetLocalizedString()
                    }, new LineSeries<DailyStatisticsAverageGameDurationViewModel>()
                    {
                        Values = statistics.DailyStatistics.Select(s =>
                            new DailyStatisticsAverageGameDurationViewModel()
                                { Date = s.Date, AverageGameDuration = s.TotalPlayTime }).ToList(),
                        ScalesYAt = 0,
                        Name = "Total play time".GetLocalizedString()
                    }
                },
                XAxis = new []
                {
                    new DateTimeAxis(TimeSpan.FromDays(1), v => v.ToString("d"))
                    {
                        Name = "Day".GetLocalizedString()
                    }
                },
                YAxis = new []
                {
                    new TimeSpanAxis(TimeSpan.FromMinutes(15), v => string.Format("{0:%h} h {0:%m} min" , v))
                    {
                        Name = "Time played".GetLocalizedString(),
                    }
                }
            },
            SpaceUsedStatistics = new ChartViewModel()
            {
                Series = statistics.SpaceUsedStatistics.AsPieSeries((dto, series) =>
                {
                    series.ToolTipLabelFormatter = point => point.Model.Title + Environment.NewLine + (string)new FileSizeFormatter().Convert(
                        point.Model.SpaceUsedBytes, typeof(string), null,
                        CultureInfo.InvariantCulture);
                    LiveCharts.Configure(config => config
                        .HasMap<GameSpaceUsedDto>((point, index) => new(index, point.SpaceUsedBytes)));
                }).ToArray()
            }
        };
    }
}