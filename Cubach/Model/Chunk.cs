using System;

namespace Cubach.Model
{
    public class Chunk
    {
        public const int Length = 32;
        public const int Width = 32;
        public const int Height = 32;

        public static readonly int SizeInBytes = Length * Width * Height * Block.SizeInBytes;

        public readonly Block[,,] Blocks = new Block[Length, Width, Height];

        public Chunk()
        {
            for (int i = 0; i < Length; ++i) {
                for (int j = 0; j < Width; ++j) {
                    for (int k = 0; k < Height; ++k) {
                        Blocks[i, j, k] = new Block(0);
                    }
                }
            }
        }

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[SizeInBytes];
            for (int x = 0; x < Length; ++x) {
                for (int y = 0; y < Width; ++y) {
                    for (int z = 0; z < Height; ++z) {
                        byte[] blockBytes = Blocks[x, y, z].GetBytes();
                        int index = (x + Length * y + Length * Width * z) * Block.SizeInBytes;
                        Array.Copy(blockBytes, 0, bytes, index, Block.SizeInBytes);
                    }
                }
            }

            return bytes;
        }

        public static Chunk Create(byte[] bytes, int startIndex = 0)
        {
            var chunk = new Chunk();
            for (int x = 0; x < Length; ++x) {
                for (int y = 0; y < Width; ++y) {
                    for (int z = 0; z < Height; ++z) {
                        int index = startIndex + (x + Length * y + Length * Width * z) * Block.SizeInBytes;
                        var block = Block.Create(bytes, index);
                        chunk.Blocks[x, y, z] = block;
                    }
                }
            }

            return chunk;
        }
    }
}
