using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Cubach.Model;

namespace Cubach
{
    public class Server : IDisposable
    {
        private readonly Configuration config;

        public World World { get; } = new World();

        public Server(Configuration config)
        {
            this.config = config;

            // Loads world from the file if it is exists. Generates and saves world to the file otherwise.
            const string worldFileName = "world.bin";
            var worldFilePath = Path.Combine(config.SavePath, worldFileName);
            if (File.Exists(worldFilePath)) {
                LoadWorld(worldFilePath);
            }
            else {
                Task.Run(() => {
                    GenerateWorld();
                    SaveWorld(worldFilePath);
                });
            }
        }

        private void LoadWorld(string path)
        {
            Console.WriteLine("[Server] Loading world...");

            using (var stream = new MemoryStream()) {
                using (var fileStream = File.OpenRead(path))
                using (var gzStream = new GZipStream(fileStream, CompressionMode.Decompress)) {
                    gzStream.CopyTo(stream);
                }

                stream.Seek(0, SeekOrigin.Begin);

                var serializer = new WorldSerializer();
                serializer.Load(stream, World);
            }

            Console.WriteLine("[Server] Loaded world");
        }

        private void SaveWorld(string path)
        {
            Console.WriteLine("[Server] Saving world...");

            using (var fileStream = File.OpenWrite(path))
            using (var gzStream = new GZipStream(fileStream, CompressionMode.Compress)) {
                var serializer = new WorldSerializer();
                serializer.Save(gzStream, World);
            }

            Console.WriteLine("[Server] Saved world");
        }

        private void GenerateWorld()
        {
            Console.WriteLine("[Server] Generating world...");

            var randomProvider = new RandomProvider(0);
            var noiseProvider = new PerlinNoise(randomProvider);
            var chunkGenerator = new ChunkGenerator(noiseProvider);
            var worldGenerator = new WorldGenerator(World, chunkGenerator);
            worldGenerator.ChunkGenerated += (s, e) => {
                Console.WriteLine($"[Server] Generated chunk {e.Chunk.X} {e.Chunk.Y} {e.Chunk.Z}");
            };
            worldGenerator.CreateChunks();

            Console.WriteLine("[Server] Generated world");
        }

        public void Run()
        {
            const int updateRate = 60;
            var sw = new Stopwatch();

            while (true) {
                sw.Restart();
                World.Update(1f / updateRate);
                sw.Stop();

                Thread.Sleep((int) (1000f / updateRate - sw.ElapsedMilliseconds));
            }
        }

        public void Dispose() { }
    }
}
