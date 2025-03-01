﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TombLauncher.ViewModels;

namespace TombLauncher.Views;

public partial class GamesDataGridView : UserControl
{
    public GamesDataGridView()
    {
        InitializeComponent();
        GamesDataGrid.LoadingRow += (sender, args) =>
        {
            var row = args.Row;

            row.PointerEntered += (o, eventArgs) =>
            {
                var dataContext = row.DataContext as GameWithStatsViewModel;
                dataContext.AreCommandsVisible = true;
            };
            row.PointerExited += (o, eventArgs) =>
            {
                var dataContext = row.DataContext as GameWithStatsViewModel;
                dataContext.AreCommandsVisible = false;
            };

            row.DoubleTapped += (o, eventArgs) =>
            {
                var dataContext = row.DataContext as GameWithStatsViewModel;
                dataContext.PlayCmd.Execute(null);
            };
        };
    }
}