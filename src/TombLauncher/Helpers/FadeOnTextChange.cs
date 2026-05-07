using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Styling;

namespace TombLauncher.Helpers;

/// <summary>
/// Attached property that plays a fade-in animation on a TextBlock whenever its Text changes.
/// Usage: helpers:FadeOnTextChange.IsEnabled="True"
/// </summary>
public class FadeOnTextChange : AvaloniaObject
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<FadeOnTextChange, TextBlock, bool>("IsEnabled");

    public static bool GetIsEnabled(TextBlock element) => element.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(TextBlock element, bool value) => element.SetValue(IsEnabledProperty, value);

    static FadeOnTextChange()
    {
        TextBlock.TextProperty.Changed.AddClassHandler<TextBlock>(OnTextChanged);
    }

    private static async void OnTextChanged(TextBlock textBlock, AvaloniaPropertyChangedEventArgs e)
    {
        if (!GetIsEnabled(textBlock)) return;

        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(500),
            Easing = new CubicEaseOut(),
            Children =
            {
                new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(Visual.OpacityProperty, 0.0) } },
                new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(Visual.OpacityProperty, 1.0) } }
            }
        };

        await animation.RunAsync(textBlock);
    }
}
