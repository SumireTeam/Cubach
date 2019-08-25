using OpenTK;

namespace Cubach.Model
{
    public class ChunkGenerator
    {
        public Chunk Create(int x, int y, int z)
        {
            var chunk = new Chunk();
            for (int i = 0; i < Chunk.Length; ++i)
            {
                for (int j = 0; j < Chunk.Width; ++j)
                {
                    for (int k = 0; k < Chunk.Height; ++k)
                    {
                        var v = new Vector3(x + i / (float)Chunk.Width, y + j / (float)Chunk.Length, z + k / (float)Chunk.Height);

                        float t = 0;
                        for (int n = 0; n < 5; ++n)
                        {
                            float d = 1 << n;
                            t += PerlinNoise.Noise3(v * d) / d;
                        }

                        int blockTypeID = t > 0 ? 1 : 0;
                        chunk.Blocks[i, j, k] = new Block(blockTypeID);
                    }
                }
            }

            return chunk;
        }
    }
}
