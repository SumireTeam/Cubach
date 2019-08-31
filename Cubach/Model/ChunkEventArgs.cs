using System;

namespace Cubach.Model
{
    public class ChunkEventArgs : EventArgs
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;
        public readonly Chunk Chunk;

        public ChunkEventArgs(int x, int y, int z, Chunk chunk)
        {
            X = x;
            Y = y;
            Z = z;
            Chunk = chunk;
        }
    }
}
