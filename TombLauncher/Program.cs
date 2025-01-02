using Avalonia;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CommunityToolkit.Mvvm.DependencyInjection;
using Serilog;
using TombLauncher.Data.Database.UnitOfWork;
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
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            var logger = Ioc.Default.GetRequiredService<ILogger>();
            logger.Fatal(e, "Fatal exception requiring restart");
            var appCrashUoW = Ioc.Default.GetService<AppCrashUnitOfWork>();
            appCrashUoW.InsertAppCrash(e);

            var thisExecutable = Assembly.GetEntryAssembly()?.Location.Replace(".dll", ".exe");
            if (thisExecutable != null && File.Exists(thisExecutable))
            {
                Process.Start(thisExecutable, args);
            }
        }
        finally
        {
            var logger = Ioc.Default.GetRequiredService<ILogger>();
            logger.Error("Application closing due to fatal exception");
            ((IDisposable)logger).Dispose();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}