using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TombLauncher.Controls;

public partial class LabeledCalendarDatePicker : UserControl
{
    public LabeledCalendarDatePicker()
    {
        InitializeComponent();
    }
    
    public static readonly StyledProperty<Infotip> InfotipProperty = AvaloniaProperty.Register<LabeledCalendarDatePicker, Infotip>(
        nameof(Infotip));

    public Infotip Infotip
    {
        get => GetValue(InfotipProperty);
        set => SetValue(InfotipProperty, value);
    }
    
    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<LabeledCalendarDatePicker, string>(
        nameof(Label));

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly StyledProperty<DateTime> SelectedDateProperty = AvaloniaProperty.Register<LabeledCalendarDatePicker, DateTime>(
        nameof(SelectedDate));

    public DateTime SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }
}