using System;
using OpenTK;

namespace Cubach.Model
{
    public class Chunk
    {
        public const int Length = 32;
        public const int Width = 32;
        public const int Height = 32;

        public static readonly Vector3 Size = new Vector3(Length, Width, Height);

        public static readonly int SizeInBytes = Length * Width * Height * Block.SizeInBytes;

        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public readonly Block[,,] Blocks = new Block[Length, Width, Height];

        public Chunk(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;

            for (var i = 0; i < Length; ++i) {
                for (var j = 0; j < Width; ++j) {
                    for (var k = 0; k < Height; ++k) {
                        Blocks[i, j, k] = new Block(0);
                    }
                }
            }
        }

        public Vector3 Min => new Vector3(X * Size.X, Y * Size.Y, Z * Size.Z);
        public Vector3 Center => Min + Size / 2;
        public Vector3 Max => Min + Size;
        public AABB AABB => new AABB(Min, Max);

        /// <summary>
        /// Returns a block at the specified indexes, or null if it is out of bounds.
        /// </summary>
        public Block? GetBlock(int i, int j, int k)
        {
            if (i < 0 || j < 0 || k < 0 || i >= Length || j >= Width || k >= Height) {
                return null;
            }

            return Blocks[i, j, k];
        }

        /// <summary>
        /// Returns a block at the specified position relative to the chunk origin, or null if it is out of bounds.
        /// </summary>
        public Block? GetBlockAt(Vector3 position)
        {
            var i = (int) Math.Floor(position.X);
            var j = (int) Math.Floor(position.Y);
            var k = (int) Math.Floor(position.Z);

            return GetBlock(i, j, k);
        }

        public byte[] GetBytes()
        {
            var bytes = new byte[SizeInBytes];
            for (var i = 0; i < Length; ++i) {
                for (var j = 0; j < Width; ++j) {
                    for (var k = 0; k < Height; ++k) {
                        var blockBytes = Blocks[i, j, k].GetBytes();
                        var index = (i + Length * j + Length * Width * k) * Block.SizeInBytes;
                        Array.Copy(blockBytes, 0, bytes, index, Block.SizeInBytes);
                    }
                }
            }

            return bytes;
        }

        public static Chunk Create(int x, int y, int z, byte[] bytes, int startIndex = 0)
        {
            var chunk = new Chunk(x, y, z);
            for (var i = 0; i < Length; ++i) {
                for (var j = 0; j < Width; ++j) {
                    for (var k = 0; k < Height; ++k) {
                        var index = startIndex + (i + Length * j + Length * Width * k) * Block.SizeInBytes;
                        var block = Block.Create(bytes, index);
                        chunk.Blocks[i, j, k] = block;
                    }
                }
            }

            return chunk;
        }
    }
}
