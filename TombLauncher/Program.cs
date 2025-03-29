using Avalonia;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using CommunityToolkit.Mvvm.DependencyInjection;
using Serilog;
using TombLauncher.Core.Exceptions;
using TombLauncher.Services;

namespace TombLauncher;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (AppRestartRequestedException ex)
        {
            var crashId = ex.CrashId;
            var appCrashHostService = Ioc.Default.GetRequiredService<AppCrashHostService>();
            appCrashHostService.MarkAsNotified(crashId).GetAwaiter().GetResult();
            var thisExecutable = Assembly.GetEntryAssembly()?.Location.Replace(".dll", ".exe");
            if (thisExecutable != null && File.Exists(thisExecutable))
            {
                Process.Start(thisExecutable, args);
            }
        }
        finally
        {
            Log.Logger.Error("Application closing due to fatal exception");
            ((IDisposable)Log.Logger).Dispose();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}