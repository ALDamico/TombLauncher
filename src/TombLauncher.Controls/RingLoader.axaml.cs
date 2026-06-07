using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace TombLauncher.Controls;

public partial class RingLoader : UserControl
{
    public RingLoader()
    {
        Diameter = 100;
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

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<RingLoader, string?>(nameof(Message));

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly StyledProperty<double> DiameterProperty = AvaloniaProperty.Register<RingLoader, double>(
        nameof(Diameter));

    public double Diameter
    {
        get => GetValue(DiameterProperty);
        set => SetValue(DiameterProperty, value);
    }
}
