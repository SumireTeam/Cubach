using System;

namespace Cubach.Model
{
    public class ChunkGenerator
    {
        public Chunk Create(int x, int y, int z)
        {
            var random = new Random(x + y << 16 + z << 32);

            var chunk = new Chunk();
            for (int i = 0; i < Chunk.Length; ++i)
            {
                for (int j = 0; j < Chunk.Width; ++j)
                {
                    for (int k = 0; k < Chunk.Height; ++k)
                    {
                        int blockTypeID = random.NextDouble() > 0.5 ? 1 : 0;
                        chunk.Blocks[i, j, k] = new Block(blockTypeID);
                    }
                }
            }

            return chunk;
        }
    }
}
