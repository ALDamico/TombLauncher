using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace TombLauncher.Controls;

public partial class RingLoader : UserControl
{
    public RingLoader()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<IBrush> RingBrushProperty =
        AvaloniaProperty.Register<RingLoader, IBrush>(nameof(RingBrush));

    public IBrush RingBrush
    {
        get => GetValue(RingBrushProperty);
        set => SetValue(RingBrushProperty, value);
    }

    public static readonly StyledProperty<IBrush> InnerRingBrushProperty =
        AvaloniaProperty.Register<RingLoader, IBrush>(nameof(InnerRingBrush));

    public IBrush InnerRingBrush
    {
        get => GetValue(InnerRingBrushProperty);
        set => SetValue(InnerRingBrushProperty, value);
    }
}
