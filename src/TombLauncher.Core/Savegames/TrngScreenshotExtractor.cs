namespace TombLauncher.Core.Savegames;

public static class TrngScreenshotExtractor
{
    private const int HeaderSize = 80;
    private const int BmpHeaderSize = 14; // magic number (2 bytes), file size (4 bytes), reserved (4 bytes), offset (4 bytes)

    public static byte[]? ExtractBitmap(byte[] data)
    {
        if (data.Length < HeaderSize + BmpHeaderSize)
            return null;

        for (var i = HeaderSize; i < data.Length - 1; i++)
        {
            if (data[i] == 'B' && data[i + 1] == 'M')
            {
                if (i + BmpHeaderSize > data.Length)
                    return null;

                var size = BitConverter.ToInt32(data, i + 2);

                if (size < BmpHeaderSize || i + size > data.Length)
                    return null;

                var bmpData = new byte[size];
                Array.Copy(data, i, bmpData, 0, size);

                return bmpData;
            }
        }

        return null;
    }
}