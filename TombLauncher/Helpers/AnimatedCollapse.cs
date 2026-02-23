using System;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TombLauncher.Helpers;

/// <summary>
/// Reusable attached property that animates a control's expand/collapse
/// using MaxHeight + Opacity transitions. 
/// Usage: helpers:AnimatedCollapse.IsOpen="{Binding SomeBool}"
/// Optional: helpers:AnimatedCollapse.MaxOpenHeight="300" (default 300)
/// </summary>
public class AnimatedCollapse : AvaloniaObject
{
    public static readonly AttachedProperty<bool> IsOpenProperty =
        AvaloniaProperty.RegisterAttached<AnimatedCollapse, Control, bool>("IsOpen", defaultValue: false);

    public static readonly AttachedProperty<double> MaxOpenHeightProperty =
        AvaloniaProperty.RegisterAttached<AnimatedCollapse, Control, double>("MaxOpenHeight", defaultValue: 300);

    public static bool GetIsOpen(Control element) => element.GetValue(IsOpenProperty);
    public static void SetIsOpen(Control element, bool value) => element.SetValue(IsOpenProperty, value);

    public static double GetMaxOpenHeight(Control element) => element.GetValue(MaxOpenHeightProperty);
    public static void SetMaxOpenHeight(Control element, double value) => element.SetValue(MaxOpenHeightProperty, value);

    static AnimatedCollapse()
    {
        IsOpenProperty.Changed.AddClassHandler<Control>(OnIsOpenChanged);
    }

    private static void OnIsOpenChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        var isOpen = (bool)e.NewValue!;
        var maxHeight = GetMaxOpenHeight(control);

        // On first attach: set initial state without transitions
        if (control.Transitions == null || control.Transitions.Count == 0)
        {
            // Set immediately without animation
            control.ClipToBounds = true;
            control.MaxHeight = isOpen ? maxHeight : 0;
            control.Opacity = isOpen ? 1 : 0;

            // Add transitions for future changes
            control.Transitions = new Avalonia.Animation.Transitions
            {
                new Avalonia.Animation.DoubleTransition
                {
                    Property = Layoutable.MaxHeightProperty,
                    Duration = TimeSpan.FromMilliseconds(250),
                    Easing = new CubicEaseOut()
                },
                new Avalonia.Animation.DoubleTransition
                {
                    Property = Visual.OpacityProperty,
                    Duration = TimeSpan.FromMilliseconds(200),
                    Easing = new CubicEaseOut()
                }
            };
            return;
        }

        control.MaxHeight = isOpen ? maxHeight : 0;
        control.Opacity = isOpen ? 1 : 0;
    }
}
