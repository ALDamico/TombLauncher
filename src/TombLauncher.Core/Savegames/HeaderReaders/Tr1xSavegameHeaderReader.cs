using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Exceptions;
using TombLauncher.Core.Utils;

namespace TombLauncher.Core.Savegames.HeaderReaders;

public class Tr1xSavegameHeaderReader : ISavegameHeaderReader
{
    private const int HeaderSize = 16;
    private const int AdditionalHeaderSize = 16;

    public SavegameHeader ReadHeader(string filepath)
    {
        var bytes = File.ReadAllBytes(filepath);
        return ReadHeader(filepath, bytes);
    }

    public SavegameHeader ReadHeader(string filepath, byte[] buf)
    {
        try
        {
            var headerBuf = buf[..HeaderSize].AsSpan();
            var trxHeader = new Tr1xHeader()
            {
                MagicNumber = BitConverter.ToInt32(headerBuf[..4]),
                InitialVersion = BitConverter.ToInt16(headerBuf[4..6]),
                Version = BitConverter.ToUInt16(headerBuf[6..8]),
                CompressedSize = BitConverter.ToInt32(headerBuf[8..12]),
                UncompressedSize = BitConverter.ToInt32(headerBuf[12..16])
            };

            string levelTitle;
            int saveNumber;

            if (buf.Length - HeaderSize != trxHeader.UncompressedSize)
            {
                var savegameDataBuf = new byte[trxHeader.UncompressedSize];
                var ms = new MemoryStream(buf.Skip(HeaderSize).Take(trxHeader.CompressedSize).ToArray());

                var zlib = new ZLibStream(ms, CompressionMode.Decompress);
                zlib.ReadExactly(savegameDataBuf);
                using var memoryStream = new MemoryStream(savegameDataBuf);
                using var bsonReader = new BsonDataReader(memoryStream);
                var jsonSerializer = new JsonSerializer();
                var savegame = jsonSerializer.Deserialize<Tr1xSavegame>(bsonReader);
                levelTitle = savegame?.LevelTitle ?? "Unknown Level";
                saveNumber = savegame.SaveCounter;
            }
            else
            {
                var dataStart = HeaderSize + trxHeader.UncompressedSize;
                var lastData = buf[dataStart..].AsSpan();
                var additionalHeader = new Tr1xAdditionalHeader()
                {
                    Flags = BitConverter.ToUInt32(lastData[..4]),
                    Counter = BitConverter.ToInt32(lastData[4..8]),
                    LevelNumber = BitConverter.ToInt32(lastData[8..12]),
                    TitleSize = BitConverter.ToInt32(lastData[12..16])
                };

                levelTitle = Encoding.ASCII.GetString(
                    lastData[AdditionalHeaderSize..(AdditionalHeaderSize + additionalHeader.TitleSize)]);
                saveNumber = additionalHeader.Counter;
            }


            return new SavegameHeader()
            {
                Filepath = filepath,
                LevelName = levelTitle,
                SaveNumber = saveNumber,
                SlotNumber = SavegameUtils.GetTr1xSlotNumber(filepath)
            };
        }
        catch (Exception ex)
        {
            throw new SavegameParseException(ex);
        }
    }

    private class Tr1xSavegame
    {
        [JsonProperty("level_title")] public string? LevelTitle { get; set; }
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

    private class Tr1xAdditionalHeader
    {
        public uint Flags { get; set; }
        public int Counter { get; set; }
        public int LevelNumber { get; set; }
        public int TitleSize { get; set; }
    }
}