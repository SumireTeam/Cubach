using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cubach.Model
{
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

            for (var i = 0; i < World.Length; ++i) {
                for (var j = 0; j < World.Width; ++j) {
                    var x = i;
                    var y = j;
                    var task = Task.Run(() => {
                        for (var k = 0; k < World.Height; ++k) {
                            var z = k;
                            var chunk = chunkGenerator.Create(x, y, z);
                            world.SetChunk(chunk);
                            ChunkGenerated(this, new ChunkEventArgs(chunk));
                        }
                    });
                    tasks.Add(task);
                }
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}
