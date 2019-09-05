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

        private IClientConnection clientConnection;

        public World World { get; } = new World();

        public Server(Configuration config)
        {
            this.config = config;

            World.ChunkUpdated += (s, e) => { clientConnection?.SendChunk(e.Chunk); };

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

            using (var fileStream = File.OpenRead(path))
            using (var gzStream = new GZipStream(fileStream, CompressionMode.Decompress)) {
                var serializer = new WorldSerializer();
                serializer.Load(gzStream, World);
            }

            Console.WriteLine("[Server] Loaded world");
        }

        private void SaveWorld(string path)
        {
            Console.WriteLine("[Server] Saving world...");

            if (!Directory.Exists(config.SavePath)) {
                Directory.CreateDirectory(config.SavePath);
            }

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
                Console.WriteLine($"[Server] Generated chunk {e.Chunk.X}, {e.Chunk.Y}, {e.Chunk.Z}");
            };
            worldGenerator.CreateChunks();

            Console.WriteLine("[Server] Generated world");
        }

        public void Connect(IClientConnection connection)
        {
            clientConnection = connection;
            clientConnection.ChunksRequestReceived += (s, e) => {
                for (var i = 0; i < World.Length; ++i) {
                    for (var j = 0; j < World.Length; ++j) {
                        for (var k = 0; k < World.Length; ++k) {
                            var chunk = World.GetChunk(i, j, k);
                            if (chunk != null) {
                                clientConnection.SendChunk(chunk);
                            }
                        }
                    }
                }
            };
        }

        public void Run()
        {
            const int updateRate = 60;
            var sw = new Stopwatch();

            while (true) {
                sw.Restart();
                clientConnection?.ProcessMessages();
                World.Update(1f / updateRate);
                sw.Stop();

                var delay = (int) (1000f / updateRate - sw.ElapsedMilliseconds);
                if (delay >= 0) {
                    Thread.Sleep(delay);
                }
            }
        }

        public void Dispose() { }
    }
}
