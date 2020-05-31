using System;

namespace Cubach.Shared
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
