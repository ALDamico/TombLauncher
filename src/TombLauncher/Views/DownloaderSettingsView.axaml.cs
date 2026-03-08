using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using TombLauncher.ViewModels.Pages.Settings;

namespace TombLauncher.Views;

public partial class DownloaderSettingsView : UserControl
{
    private ListBoxItem? _draggedItem;
    private int _dragStartIndex = -1;
    private Point _pressPosition;
    private bool _isDragging;
    private const double DragThreshold = 10;
    private int _lastDropTargetIndex = -1;

    static DownloaderSettingsView()
    {
        ReorderFormat =
            DataFormat.CreateStringApplicationFormat("tomb.launcher.downloader.reorder");
    }

    private static readonly DataFormat<string> ReorderFormat;

    public DownloaderSettingsView()
    {
        InitializeComponent();
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _pressPosition = e.GetPosition(this);
            var listBoxItem = (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>();
            if (listBoxItem != null)
            {
                _draggedItem = listBoxItem;
                _dragStartIndex = DownloaderList.IndexFromContainer(listBoxItem);
            }
        }
    }

    private async void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_draggedItem == null || _isDragging) return;

        var currentPosition = e.GetPosition(this);
        var diff = currentPosition - _pressPosition;
        if (Math.Abs(diff.Y) > DragThreshold)
        {
            _isDragging = true;
            _draggedItem.Opacity = 0.4;
            _draggedItem.RenderTransform = new ScaleTransform(0.97, 0.97);

            var item = new DataTransferItem();
            item.Set(ReorderFormat, _dragStartIndex.ToString());
            var data = new DataTransfer();
            data.Add(item);
            await DragDrop.DoDragDropAsync(e, data, DragDropEffects.Move);

            // Reset visual state after drag ends
            ResetDragVisuals();
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        ResetDragVisuals();
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (!_isDragging)
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        e.DragEffects = DragDropEffects.Move;

        // Calculate drop target and highlight it
        var targetIndex = GetDropTargetIndex(e.GetPosition(DownloaderList));
        if (targetIndex != _lastDropTargetIndex)
        {
            ClearDropIndicators();
            _lastDropTargetIndex = targetIndex;
            HighlightDropTarget(targetIndex);
        }
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        if (_dragStartIndex < 0) return;

        ClearDropIndicators();
        var newIndex = GetDropTargetIndex(e.GetPosition(DownloaderList));

        if (DataContext is DownloaderSettingsViewModel vm)
        {
            vm.Reorder(_dragStartIndex, newIndex);
        }

        ResetDragVisuals();
    }

    private int GetDropTargetIndex(Point position)
    {
        for (var i = 0; i < DownloaderList.ItemCount; i++)
        {
            var container = DownloaderList.ContainerFromIndex(i);
            if (container is not ListBoxItem listBoxItem) continue;

            var itemBounds = listBoxItem.Bounds;
            var itemMidY = itemBounds.Y + itemBounds.Height / 2;

            if (position.Y < itemMidY)
                return i;
        }

        return DownloaderList.ItemCount - 1;
    }

    private void HighlightDropTarget(int targetIndex)
    {
        var container = DownloaderList.ContainerFromIndex(targetIndex);
        if (container is ListBoxItem item && item != _draggedItem)
        {
            // Show a top border on the target item as drop indicator
            var border = item.FindDescendantOfType<Border>();
            if (border != null)
            {
                border.BorderThickness = new Thickness(1, 3, 1, 1);
            }
        }
    }

    private void ClearDropIndicators()
    {
        for (var i = 0; i < DownloaderList.ItemCount; i++)
        {
            var container = DownloaderList.ContainerFromIndex(i);
            if (container is not ListBoxItem item) continue;

            var border = item.FindDescendantOfType<Border>();
            if (border != null)
            {
                border.BorderThickness = new Thickness(1);
            }
        }

        _lastDropTargetIndex = -1;
    }

    private void ResetDragVisuals()
    {
        if (_draggedItem != null)
        {
            _draggedItem.Opacity = 1.0;
            _draggedItem.RenderTransform = null;
        }

        ClearDropIndicators();
        _isDragging = false;
        _draggedItem = null;
        _dragStartIndex = -1;
    }
}