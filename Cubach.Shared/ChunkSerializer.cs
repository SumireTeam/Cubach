using System;

namespace Cubach.Shared
{
    public class ChunkSerializer
    {
        private static readonly int ChunkSizeInBytes = Chunk.Length * Chunk.Width * Chunk.Height * Block.SizeInBytes;

        public byte[] ToByteArray(Chunk chunk)
        {
            var bytes = new byte[ChunkSizeInBytes];
            for (var i = 0; i < Chunk.Length; ++i) {
                for (var j = 0; j < Chunk.Width; ++j) {
                    for (var k = 0; k < Chunk.Height; ++k) {
                        var blockBytes = chunk.Blocks[i, j, k].GetBytes();
                        var index = (i + Chunk.Length * j + Chunk.Length * Chunk.Width * k) * Block.SizeInBytes;
                        Array.Copy(blockBytes, 0, bytes, index, Block.SizeInBytes);
                    }
                }
            }

            return bytes;
        }

        public Chunk FromByteArray(int x, int y, int z, byte[] bytes, int startIndex = 0)
        {
            var chunk = new Chunk(x, y, z);
            for (var i = 0; i < Chunk.Length; ++i) {
                for (var j = 0; j < Chunk.Width; ++j) {
                    for (var k = 0; k < Chunk.Height; ++k) {
                        var index = (i + Chunk.Length * j + Chunk.Length * Chunk.Width * k) * Block.SizeInBytes;
                        index += startIndex;

                        var block = Block.Create(bytes, index);
                        chunk.Blocks[i, j, k] = block;
                    }
                }
            }

            return chunk;
        }
    }
}
