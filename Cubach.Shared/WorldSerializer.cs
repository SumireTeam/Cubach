using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Cubach.Shared
{
    public enum DataType : byte
    {
        ByteArray = 1,
    }

    public class WorldSerializer
    {
        private readonly ChunkSerializer chunkSerializer = new ChunkSerializer();

        public void Save(Stream stream, World world)
        {
            using (var writer = new BinaryWriter(stream)) {
                for (var i = 0; i < World.Length; ++i) {
                    for (var j = 0; j < World.Width; ++j) {
                        for (var k = 0; k < World.Height; ++k) {
                            var key = $"chunk:{i}:{j}:{k}:blocks";
                            writer.Write(key);

                            var chunk = world.GetChunk(i, j, k);
                            var value = chunkSerializer.ToByteArray(chunk);
                            writer.Write((byte) DataType.ByteArray);
                            writer.Write(value.Length);
                            writer.Write(value);
                        }
                    }
                }

                writer.Write("end");
            }
        }

        public void Load(Stream stream, World world)
        {
            using (var reader = new BinaryReader(stream)) {
                string key;
                while ((key = reader.ReadString()) != "end") {
                    var type = (DataType) reader.ReadByte();
                    switch (type) {
                        case DataType.ByteArray:
                            var dataLength = reader.ReadInt32();
                            var data = reader.ReadBytes(dataLength);

                            var blocksMatch = Regex.Match(key, "^chunk:(\\d+):(\\d+):(\\d+):blocks$");
                            if (blocksMatch.Success) {
                                var i = int.Parse(blocksMatch.Groups[1].Value);
                                var j = int.Parse(blocksMatch.Groups[2].Value);
                                var k = int.Parse(blocksMatch.Groups[3].Value);
                                var chunk = chunkSerializer.FromByteArray(i, j, k, data);
                                world.SetChunk(chunk);
                            }

                            break;

                        default:
                            throw new NotImplementedException($"Unknown data type: {(byte) type}");
                    }
                }
            }
        }
    }
}
