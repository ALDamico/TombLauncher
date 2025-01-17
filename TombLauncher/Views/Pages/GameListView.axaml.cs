﻿using Avalonia.Controls;
using TombLauncher.ViewModels;

namespace TombLauncher.Views.Pages;

public partial class GameListView : UserControl
{
    public GameListView()
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