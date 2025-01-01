using System.Threading.Tasks;
using AsyncImageLoader;
using Avalonia.Media.Imaging;

namespace TombLauncher.Utils;

public class ByteArrayImageLoader
{
    public void Dispose()
    {
        throw new System.NotImplementedException();
    }

    public Task<Bitmap> ProvideImageAsync(byte[] url)
    {
        return Task.FromResult(ImageUtils.ToBitmap(url));
    }
}