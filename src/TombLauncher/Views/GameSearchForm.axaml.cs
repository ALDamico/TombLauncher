using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using TombLauncher.Contracts.Enums;
using TombLauncher.Data.Models;
using TombLauncher.ViewModels;

namespace TombLauncher.Views;

public partial class GameSearchForm : UserControl
{
    public GameSearchForm()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<IList<EnumViewModel<GameEngine>>> AvailableEnginesProperty = AvaloniaProperty.Register<GameSearchForm, IList<EnumViewModel<GameEngine>>>(
        nameof(AvailableEngines));

    public IList<EnumViewModel<GameEngine>> AvailableEngines
    {
        get => GetValue(AvailableEnginesProperty);
        set => SetValue(AvailableEnginesProperty, value);
    }

    public static readonly StyledProperty<IList<EnumViewModel<GameDifficulty>>> AvailableDifficultiesProperty = AvaloniaProperty.Register<GameSearchForm, IList<EnumViewModel<GameDifficulty>>>(
        nameof(AvailableDifficulties));

    public IList<EnumViewModel<GameDifficulty>> AvailableDifficulties
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

    public static readonly StyledProperty<ICommand> SearchCommandProperty = AvaloniaProperty.Register<GameSearchForm, ICommand>(
        nameof(SearchCommand));

    public ICommand SearchCommand
    {
        get => GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> ClearFiltersCommandProperty = AvaloniaProperty.Register<GameSearchForm, ICommand>(
        nameof(ClearFiltersCommand));

    public ICommand ClearFiltersCommand
    {
        get => GetValue(ClearFiltersCommandProperty);
        set => SetValue(ClearFiltersCommandProperty, value);
    }
}