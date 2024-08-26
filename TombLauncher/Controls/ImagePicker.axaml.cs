using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using JamSoft.AvaloniaUI.Dialogs;

namespace TombLauncher.Controls;

public partial class ImagePicker : UserControl
{
    public ImagePicker()
    {
        InitializeComponent();
        Text.IsVisible = false;
        //Image.Effect = new BlurEffect(){Radius = 0, Transitions = new Transitions(){new EffectTransition(){Property = EffectProperty, Duration = TimeSpan.FromMilliseconds(5000), Easing = new LinearEasing()}}};;
    }

    private void ImagePicker_OnPointerEntered(object sender, PointerEventArgs e)
    {
        //Image.Effect = new BlurEffect(){Radius = 15, Transitions = new Transitions(){new EffectTransition(){Property = EffectProperty, Duration = TimeSpan.FromMilliseconds(5000), Easing = new LinearEasing()}}};
        Text.IsVisible = true;
    }

    private void ImagePicker_OnPointerExited(object sender, PointerEventArgs e)
    {
        //Image.Effect = new BlurEffect(){Radius = 0, Transitions = new Transitions(){new EffectTransition(){Property = EffectProperty, Duration = TimeSpan.FromMilliseconds(5000), Easing = new LinearEasing()}}};;
        Text.IsVisible = false;
    }

    private async void ImagePicker_OnTapped(object sender, TappedEventArgs e)
    {
        var selectedFile = await DialogService.OpenFile("Select an image", new List<FilePickerFileType>()
        {
            new FilePickerFileType("Image files")
            {
                Patterns = new List<string>()
                {
                    "*.bmp",
                    "*.png",
                    "*.jpg"
                }
            }
        }); // TODO Make this a prop
        if (!string.IsNullOrWhiteSpace(selectedFile))
        {
            var newValue = new Bitmap(selectedFile);
            var oldValue = Source;
            Source = newValue;
            
            this.OnPropertyChanged(new AvaloniaPropertyChangedEventArgs<IImage>(this, SourceProperty, new Optional<IImage>(oldValue), new BindingValue<IImage>(newValue), BindingPriority.Inherited));
        }
    }
    
    public IDialogService DialogService
    {
        get => GetValue(DialogServiceProperty);
        set => SetValue(DialogServiceProperty, value);
    }

    public IImage Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly StyledProperty<IDialogService> DialogServiceProperty =
        AvaloniaProperty.Register<ImagePicker, IDialogService>(nameof(DialogService));
    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<Image, IImage?>(nameof(Source), default, true, BindingMode.TwoWay);
}