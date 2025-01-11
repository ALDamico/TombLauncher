using System;
using System.IO;
using System.Reflection;
using AutoMapper;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Core.Utils;
using TombLauncher.Data.Database.UnitOfWork;
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
    public Version GetApplicationVersion()
    {
        return Assembly.GetEntryAssembly()?.GetName().Version;
    }

    public Version GetNetVersion()
    {
        return Environment.Version;
    }

    public long GetDatabaseSize()
    {
        var databasePath = _settingsService.GetDatabasePath();
        var fileInfo = new FileInfo(databasePath);
        return fileInfo.Length;
    }

    public long GetGamesSize()
    {
        var gamesFolder = PathUtils.GetGamesFolder();
        var files = Directory.EnumerateFiles(gamesFolder, "*.*", SearchOption.AllDirectories);
        var runningTotal = 0L;
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            runningTotal += fileInfo.Length;
        }

        return runningTotal;
    }

    public StatisticsViewModel GetStatistics()
    {
        var statistics = _gamesUnitOfWork.GetStatistics();
        return _mapper.Map<StatisticsViewModel>(statistics);
    }
}