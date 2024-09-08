using System;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;

namespace TombLauncher.Utils;

public static class ImageUtils
{
    public static byte[] ToByteArray(Bitmap bitmap)
    {
        byte[] arr = null;
        if (bitmap == null)
        {
            return arr;
        }
        
        using var memoryStream = new MemoryStream();
        bitmap.Save(memoryStream);
        return memoryStream.ToArray();
    }

    public static Bitmap ToBitmap(byte[] byteArr)
    {
        if (byteArr == null)
        {
            return null;
        }

        var memoryStream = new MemoryStream(byteArr);
        return new Bitmap(memoryStream);
    }

    public static Bitmap NullBitmap
    {
        get
        {
            if (App.Current.ActualThemeVariant == ThemeVariant.Dark)
            {
                return new Bitmap(
                    AssetLoader.Open(new Uri("avares://TombLauncher/Assets/unknown-title-pic-sm-light.png")));
            }

            return new Bitmap(AssetLoader.Open(new Uri("avares://TombLauncher/Assets/unknown-title-pic-sm.png")));
        }
    }
}