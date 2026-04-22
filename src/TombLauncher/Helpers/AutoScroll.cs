using Avalonia;
using Avalonia.Controls;

namespace TombLauncher.Helpers;

/// <summary>
/// Attached property that auto-scrolls a ScrollViewer to the bottom when new content arrives,
/// but only if the user was already near the bottom (sticky scroll).
/// Usage: helpers:AutoScroll.IsEnabled="True"
/// </summary>
public class AutoScroll : AvaloniaObject
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<AutoScroll, ScrollViewer, bool>("IsEnabled");

    public static bool GetIsEnabled(ScrollViewer element) => element.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(ScrollViewer element, bool value) => element.SetValue(IsEnabledProperty, value);

    static AutoScroll()
    {
        IsEnabledProperty.Changed.AddClassHandler<ScrollViewer>(OnIsEnabledChanged);
    }

    private static void OnIsEnabledChanged(ScrollViewer sv, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
            sv.ScrollChanged += OnScrollChanged;
        else
            sv.ScrollChanged -= OnScrollChanged;
    }

    private static void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (e.ExtentDelta.Y <= 0 && e.ViewportDelta.Y <= 0) return;

        var sv = (ScrollViewer)sender!;
        var prevExtent = sv.Extent.Height - e.ExtentDelta.Y;
        var prevViewport = sv.Viewport.Height - e.ViewportDelta.Y;
        var prevOffset = sv.Offset.Y - e.OffsetDelta.Y;

        var wasAtBottom = prevOffset + prevViewport >= prevExtent - 20;
        if (wasAtBottom)
            sv.ScrollToEnd();
    }
}
