﻿using Avalonia;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Database.UnitOfWork;
using TombLauncher.Navigation;

namespace TombLauncher;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
#if !DEBUG
        try
        {
#endif
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
#if !DEBUG
        }
        catch (Exception e)
        {
            var appCrashUoW = Ioc.Default.GetService<AppCrashUnitOfWork>();
            appCrashUoW.InsertAppCrash(e);

            var thisExecutable = Assembly.GetEntryAssembly()?.Location.Replace(".dll", ".exe");
            if (thisExecutable != null && File.Exists(thisExecutable))
            {
                Process.Start(thisExecutable, args);
            }
        }
#endif
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    public static NavigationManager NavigationManager => Ioc.Default.GetRequiredService<NavigationManager>();
}