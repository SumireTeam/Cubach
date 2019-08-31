using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    public class WorldGenerator
    {
        private readonly World world;
        private readonly ChunkGenerator chunkGenerator;

        public event EventHandler<ChunkEventArgs> ChunkGenerated = (s, e) => { };

        public WorldGenerator(World world, ChunkGenerator chunkGenerator)
        {
            this.world = world;
            this.chunkGenerator = chunkGenerator;
        }

        public void CreateChunks()
        {
            var tasks = new List<Task>();

            for (int i = 0; i < World.Length; ++i) {
                for (int j = 0; j < World.Width; ++j) {
                    var cx = i;
                    var cy = j;
                    var task = Task.Run(() => {
                        for (int k = 0; k < World.Height; ++k) {
                            var cz = k;
                            var chunk = chunkGenerator.Create(cx, cy, cz);
                            world.Chunks[cx, cy, cz] = chunk;
                            ChunkGenerated(this, new ChunkEventArgs(cx, cy, cz, chunk));
                        }
                    });
                    tasks.Add(task);
                }
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}
