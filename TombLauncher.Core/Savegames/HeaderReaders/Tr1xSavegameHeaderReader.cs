using System.IO.Compression;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Utils;

namespace TombLauncher.Core.Savegames.HeaderReaders;

public class Tr1xSavegameHeaderReader : ISavegameHeaderReader
{
    private const int HeaderSize = 16;
    public SavegameHeader ReadHeader(string filepath)
    {
        using (var fs = File.OpenRead(filepath))
        {

            var headerBuf = new byte[HeaderSize];
            fs.Read(headerBuf);
            var trxHeader = new Tr1xHeader()
            {
                MagicNumber = BitConverter.ToInt32(headerBuf[..4]),
                InitialVersion = BitConverter.ToInt16(headerBuf[4..6]),
                Version = BitConverter.ToUInt16(headerBuf[6..8]),
                CompressedSize = BitConverter.ToInt32(headerBuf[8..12]),
                UncompressedSize = BitConverter.ToInt32(headerBuf[12..16])
            };

            var savegameDataBuf = new byte[trxHeader.CompressedSize];
            var memoryStream = new MemoryStream(savegameDataBuf);
            
            fs.CopyTo(memoryStream);

            var zlib = new ZLibStream(fs, CompressionMode.Decompress);
            zlib.ReadExactly(savegameDataBuf);
            
        }
    }

    public SavegameHeader ReadHeader(string filepath, byte[] buf)
    {
        throw new NotImplementedException();
    }

    private class Tr1xSavegame
    {
        public string LevelTitle { get; set; }
        public int SaveCounter { get; set; }
    }

    private class Tr1xHeader
    {
        public int MagicNumber { get; set; } // Letters T1MB saved as a 32-bit integer
        public short InitialVersion { get; set; }
        public ushort Version { get; set; }
        public int CompressedSize { get; set; }
        public int UncompressedSize { get; set; }
    }
}