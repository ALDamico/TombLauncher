using System;
using Avalonia;
using Avalonia.Controls;

namespace TombLauncher.Views;

public partial class SplashScreen : Window
{
    public SplashScreen()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<Version?> VersionProperty = AvaloniaProperty.Register<SplashScreen, Version?>(
        nameof(Version));

    public Version? Version
    {
        get => GetValue(VersionProperty);
        set => SetValue(VersionProperty, value);
    }

    public static readonly StyledProperty<string> StatusMessageProperty = AvaloniaProperty.Register<SplashScreen, string>(
        nameof(StatusMessage), defaultValue: "Loading...");

    public string StatusMessage
    {
        get => GetValue(StatusMessageProperty);
        set => SetValue(StatusMessageProperty, value);
    }
}
