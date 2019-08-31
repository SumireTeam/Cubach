using OpenTK;

namespace Cubach.Model
{
    public class ChunkGenerator
    {
        private readonly INoiseProvider noiseProvider;

        public ChunkGenerator(INoiseProvider noiseProvider) => this.noiseProvider = noiseProvider;

        private float GetHeight(int x, int y)
        {
            const float baseHeight = World.Height * Chunk.Height / 2f;
            float height = baseHeight;

            const float frequency = 128;
            const float amplitude = 32;
            const int octaves = 5;

            for (int n = 0; n < octaves; ++n) {
                float d = 1 << n;
                height += noiseProvider.Noise(new Vector2(x, y) * d / frequency) * amplitude / d;
            }

            return height;
        }

        public Chunk Create(int cx, int cy, int cz)
        {
            var chunk = new Chunk();
            for (int i = 0; i < Chunk.Length; ++i) {
                var x = cx * Chunk.Length + i;

                for (int j = 0; j < Chunk.Width; ++j) {
                    var y = cy * Chunk.Width + j;
                    var height = GetHeight(x, y);

                    for (int k = 0; k < Chunk.Height; ++k) {
                        var z = cz * Chunk.Height + k;
                        var blockTypeId = 0;

                        if (z < height) {
                            blockTypeId = 1;

                            // Create caves.
                            float r = 0;
                            for (int n = 0; n < 5; ++n) {
                                float d = 1 << n;
                                r += noiseProvider.Noise(new Vector3(x, y, z) * d / 100) / d;
                            }

                            if (-0.05 < r && r < 0.05) {
                                blockTypeId = 0;
                            }
                        }

                        chunk.Blocks[i, j, k] = new Block(blockTypeId);
                    }
                }
            }

            return chunk;
        }
    }
}
