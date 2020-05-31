using System;
using OpenTK;
using SMath = System.Math;

namespace Cubach.Shared
{
    public class World
    {
        public const int Length = 16;
        public const int Width = 16;
        public const int Height = 8;

        private readonly Chunk[,,] chunks = new Chunk[Length, Width, Height];

        public readonly Player Player;

        public event EventHandler<ChunkEventArgs> ChunkUpdated = (s, e) => { };

        public World()
        {
            for (var i = 0; i < Length; ++i)
            {
                for (var j = 0; j < Width; ++j)
                {
                    for (var k = 0; k < Height; ++k)
                    {
                        chunks[i, j, k] = new Chunk(i, j, k);
                    }
                }
            }

            var position = new Vector3(
                Chunk.Length * Length / 2f,
                Chunk.Width * Width / 2f,
                Chunk.Height * Height * 3 / 4f
            );
            Player = new Player(position);
        }

        /// <summary>
        /// Returns a chunk at the specified indexes, or null if it is out of bounds.
        /// </summary>
        public Chunk GetChunk(int i, int j, int k)
        {
            if (i < 0 || j < 0 || k < 0 || i >= Length || j >= Width || k >= Height)
            {
                return null;
            }

            return chunks[i, j, k];
        }

        /// <summary>
        /// Returns a chunk at the specified position, or null if it is out of bounds.
        /// </summary>
        public Chunk GetChunkAt(Vector3 position)
        {
            var i = (int)SMath.Floor(position.X / Chunk.Length);
            var j = (int)SMath.Floor(position.Y / Chunk.Width);
            var k = (int)SMath.Floor(position.Z / Chunk.Height);

            return GetChunk(i, j, k);
        }

        public Block? GetBlockAt(Vector3 position)
        {
            var chunk = GetChunkAt(position);
            if (chunk == null)
            {
                return null;
            }

            var i = (int)SMath.Floor(position.X) % Chunk.Length;
            var j = (int)SMath.Floor(position.Y) % Chunk.Width;
            var k = (int)SMath.Floor(position.Z) % Chunk.Height;

            return chunk.GetBlock(i, j, k);
        }

        public void SetChunk(Chunk chunk)
        {
            chunks[chunk.X, chunk.Y, chunk.Z] = chunk;
            ChunkUpdated(this, new ChunkEventArgs(chunk));
        }

        public void Update(float elapsedTime)
        {
            Player.Position += Player.Speed * elapsedTime;
            //Player.Speed += Vector3.UnitZ * -9.81f * elapsedTime;
        }
    }
}
