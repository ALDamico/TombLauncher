using System.IO.Compression;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Utils;

namespace TombLauncher.Core.Savegames.HeaderReaders;

public class Tr1xSavegameHeaderReader : ISavegameHeaderReader
{
    private const int HeaderSize = 16;

    public SavegameHeader ReadHeader(string filepath)
    {
        var bytes = File.ReadAllBytes(filepath);
        return ReadHeader(filepath, bytes);
    }

    public SavegameHeader ReadHeader(string filepath, byte[] buf)
    {
        var headerBuf = buf[..HeaderSize];
        var trxHeader = new Tr1xHeader()
        {
            MagicNumber = BitConverter.ToInt32(headerBuf.AsSpan()[..4]),
            InitialVersion = BitConverter.ToInt16(headerBuf.AsSpan()[4..6]),
            Version = BitConverter.ToUInt16(headerBuf.AsSpan()[6..8]),
            CompressedSize = BitConverter.ToInt32(headerBuf.AsSpan()[8..12]),
            UncompressedSize = BitConverter.ToInt32(headerBuf.AsSpan()[12..16])
        };

        var savegameDataBuf = new byte[trxHeader.UncompressedSize];
        var ms = new MemoryStream(buf[HeaderSize..]);

        var zlib = new ZLibStream(ms, CompressionMode.Decompress);
        var bytesRead = zlib.Read(savegameDataBuf);

        if (bytesRead != trxHeader.UncompressedSize)
        {
            throw new InvalidOperationException();
        }

        using var memoryStream = new MemoryStream(savegameDataBuf);
        using var bsonReader = new BsonDataReader(memoryStream);
        var jsonSerializer = new JsonSerializer();
        var savegame = jsonSerializer.Deserialize<Tr1xSavegame>(bsonReader);
        return new SavegameHeader()
        {
            Filepath = filepath,
            LevelName = savegame.LevelTitle,
            SaveNumber = savegame.SaveCounter,
            SlotNumber = SavegameUtils.GetTr1xSlotNumber(filepath)
        };
    }

    private class Tr1xSavegame
    {
        [JsonProperty("level_title")] public string LevelTitle { get; set; }
        [JsonProperty("save_counter")] public int SaveCounter { get; set; }
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