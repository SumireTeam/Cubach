using Cubach.Shared.Math;
using OpenTK;
using SMath = System.Math;

namespace Cubach.Shared
{
    public class ChunkGenerator
    {
        private readonly INoiseProvider noiseProvider;

        public ChunkGenerator(INoiseProvider noiseProvider) => this.noiseProvider = noiseProvider;

        private float GetHeight(int x, int y)
        {
            const float baseHeight = World.Height * Chunk.Height / 4f;
            var position = new Vector2(x, y);

            var worldLength = Chunk.Length * World.Length;
            var worldWidth = Chunk.Width * World.Width;

            var worldHalfLength = worldLength / 2f;
            var worldHalfWidth = worldWidth / 2f;

            var worldCenter = new Vector2(worldHalfLength, Chunk.Width * World.Width / 2f);
            var worldCenterDistance = MathUtils.TaxicabDistance(position, worldCenter);

            var distanceClamped = SMath.Min(worldCenterDistance, worldHalfLength);
            var height = 2 * baseHeight;
            height -= (float)(SMath.Cos(distanceClamped * SMath.PI / worldHalfLength - SMath.PI) + 1) * baseHeight / 2f;

            const float frequency = 128;
            const float amplitude = 32;
            const int octaves = 5;

            for (int n = 0; n < octaves; ++n)
            {
                float d = 1 << n;
                height += noiseProvider.Noise(new Vector2(x, y) * d / frequency) * amplitude / d;
            }

            return height;
        }

        public Chunk Create(int cx, int cy, int cz)
        {
            const float baseHeight = World.Height * Chunk.Height / 4f;
            const float seaLevel = baseHeight * 1.25f;

            var airId = BlockType.GetIdByName("Air");
            var stoneId = BlockType.GetIdByName("Stone");
            var dirtId = BlockType.GetIdByName("Dirt");
            var grassId = BlockType.GetIdByName("Grass");
            var sandId = BlockType.GetIdByName("Sand");
            var waterId = BlockType.GetIdByName("Water");

            var chunk = new Chunk(cx, cy, cz);
            for (var i = 0; i < Chunk.Length; ++i)
            {
                var x = cx * Chunk.Length + i;

                for (var j = 0; j < Chunk.Width; ++j)
                {
                    var y = cy * Chunk.Width + j;
                    var height = GetHeight(x, y);

                    for (var k = 0; k < Chunk.Height; ++k)
                    {
                        var z = cz * Chunk.Height + k;

                        var blockTypeId = airId;
                        if (z < seaLevel)
                        {
                            blockTypeId = waterId;
                        }

                        if (z <= (int)height)
                        {
                            // Make top layer of grass, 5 layers of dirt, and stone to the bottom.
                            if (z == (int)height)
                            {
                                blockTypeId = height > seaLevel ? grassId : sandId;
                            }
                            else if (z > (int)height - 5)
                            {
                                blockTypeId = height > seaLevel ? dirtId : sandId;
                            }
                            else
                            {
                                blockTypeId = stoneId;
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
