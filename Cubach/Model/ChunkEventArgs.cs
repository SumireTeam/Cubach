using System;

namespace Cubach.Model
{
    public class ChunkEventArgs : EventArgs
    {
        public readonly Chunk Chunk;

        public ChunkEventArgs(Chunk chunk)
        {
            Chunk = chunk;
        }
    }
}
