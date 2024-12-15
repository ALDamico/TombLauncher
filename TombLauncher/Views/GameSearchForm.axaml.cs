using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TombLauncher.Contracts.Enums;
using TombLauncher.Data.Models;

namespace TombLauncher.Views;

public partial class GameSearchForm : UserControl
{
    public GameSearchForm()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<List<GameEngine>> AvailableEnginesProperty = AvaloniaProperty.Register<GameSearchForm, List<GameEngine>>(
        nameof(AvailableEngines));

    public List<GameEngine> AvailableEngines
    {
        get => GetValue(AvailableEnginesProperty);
        set => SetValue(AvailableEnginesProperty, value);
    }

    public static readonly StyledProperty<List<GameDifficulty>> AvailableDifficultiesProperty = AvaloniaProperty.Register<GameSearchForm, List<GameDifficulty>>(
        nameof(AvailableDifficulties));

    public List<GameDifficulty> AvailableDifficulties
    {
        get => GetValue(AvailableDifficultiesProperty);
        set => SetValue(AvailableDifficultiesProperty, value);
    }

    public static readonly StyledProperty<List<GameLength>> AvailableLengthsProperty = AvaloniaProperty.Register<GameSearchForm, List<GameLength>>(
        nameof(AvailableLengths));

    public List<GameLength> AvailableLengths
    {
        get => GetValue(AvailableLengthsProperty);
        set => SetValue(AvailableLengthsProperty, value);
    }
}