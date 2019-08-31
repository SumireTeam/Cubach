using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Cubach.Model
{
    public enum DataType : byte
    {
        ByteArray = 1,
    }

    public class World
    {
        public const int Length = 8;
        public const int Width = 8;
        public const int Height = 8;

        public readonly Chunk[,,] Chunks = new Chunk[Length, Width, Height];

        public event EventHandler<ChunkEventArgs> ChunkUpdated = (s, e) => { };

        public World()
        {
            for (int i = 0; i < Length; ++i) {
                for (int j = 0; j < Width; ++j) {
                    for (int k = 0; k < Height; ++k) {
                        Chunks[i, j, k] = new Chunk();
                    }
                }
            }
        }

        public void Save(Stream stream)
        {
            using (var writer = new BinaryWriter(stream)) {
                for (int x = 0; x < Length; ++x) {
                    for (int y = 0; y < Width; ++y) {
                        for (int z = 0; z < Height; ++z) {
                            var key = $"chunk:{x}:{y}:{z}:blocks";
                            writer.Write(key);

                            var chunk = Chunks[x, y, z];
                            var value = chunk.GetBytes();
                            writer.Write((byte) DataType.ByteArray);
                            writer.Write(value.Length);
                            writer.Write(value);
                        }
                    }
                }
            }
        }

        public void Load(Stream stream)
        {
            using (var reader = new BinaryReader(stream)) {
                var length = stream.Length;
                while (stream.Position < length) {
                    var key = reader.ReadString();
                    var type = (DataType) reader.ReadByte();
                    switch (type) {
                        case DataType.ByteArray:
                            var dataLength = reader.ReadInt32();
                            var data = reader.ReadBytes(dataLength);

                            var blocksMatch = Regex.Match(key, "^chunk:(\\d+):(\\d+):(\\d+):blocks$");
                            if (blocksMatch.Success) {
                                var x = int.Parse(blocksMatch.Groups[1].Value);
                                var y = int.Parse(blocksMatch.Groups[2].Value);
                                var z = int.Parse(blocksMatch.Groups[3].Value);
                                var chunk = Chunk.Create(data);
                                Chunks[x, y, z] = chunk;
                                ChunkUpdated(this, new ChunkEventArgs(x, y, z, chunk));
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
