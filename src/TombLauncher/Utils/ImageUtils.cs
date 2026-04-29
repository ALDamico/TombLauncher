using System;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;

namespace TombLauncher.Utils;

public static class ImageUtils
{
    public static byte[] ToByteArray(Bitmap? bitmap)
    {
        byte[] arr = [];
        if (bitmap == null)
        {
            return arr;
        }
        
        using var memoryStream = new MemoryStream();
        bitmap.Save(memoryStream);
        return memoryStream.ToArray();
    }

    public static Bitmap? ToBitmap(byte[]? byteArr)
    {
        if (byteArr == null || byteArr.Length == 0)
            return null;
        
        using var memoryStream = new MemoryStream(byteArr);
        return new Bitmap(memoryStream);
    }

    public static Bitmap NullBitmap
    {
        get
        {
            if (Application.Current?.ActualThemeVariant == ThemeVariant.Dark)
            {
                return new Bitmap(
                    AssetLoader.Open(new Uri("avares://TombLauncher/Assets/unknown-title-pic-sm-light.png")));
            }

            return new Bitmap(AssetLoader.Open(new Uri("avares://TombLauncher/Assets/unknown-title-pic-sm.png")));
        }
    }
}