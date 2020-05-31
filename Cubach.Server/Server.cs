using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Cubach.Server.Network;
using Cubach.Shared;
using Cubach.Shared.Math;
using Cubach.Shared.Network;

namespace Cubach.Server
{
    public class Server : IDisposable
    {
        private readonly Configuration config;

        private IClientManager clientManager;

        public World World { get; } = new World();

        public Server(Configuration config)
        {
            this.config = config;

            World.ChunkUpdated += (s, e) => { clientManager?.SendChunk(e.Chunk); };

            // Loads world from the file if it is exists. Generates and saves world to the file otherwise.
            const string worldFileName = "world.bin";
            var worldFilePath = Path.Combine(config.SavePath, worldFileName);
            if (File.Exists(worldFilePath))
            {
                LoadWorld(worldFilePath);
            }
            else
            {
                Task.Run(() =>
                {
                    GenerateWorld();
                    SaveWorld(worldFilePath);
                });
            }
        }

        private void LoadWorld(string path)
        {
            Console.WriteLine("Loading world...");

            using (var fileStream = File.OpenRead(path))
            using (var gzStream = new GZipStream(fileStream, CompressionMode.Decompress))
            {
                var serializer = new WorldSerializer();
                serializer.Load(gzStream, World);
            }

            Console.WriteLine("Loaded world");
        }

        private void SaveWorld(string path)
        {
            Console.WriteLine("Saving world...");

            if (!Directory.Exists(config.SavePath))
            {
                Directory.CreateDirectory(config.SavePath);
            }

            using (var fileStream = File.OpenWrite(path))
            using (var gzStream = new GZipStream(fileStream, CompressionMode.Compress))
            {
                var serializer = new WorldSerializer();
                serializer.Save(gzStream, World);
            }

            Console.WriteLine("Saved world");
        }

        private void GenerateWorld()
        {
            Console.WriteLine("Generating world...");

            var randomProvider = new RandomProvider(0);
            var noiseProvider = new PerlinNoise(randomProvider);
            var chunkGenerator = new ChunkGenerator(noiseProvider);
            var worldGenerator = new WorldGenerator(World, chunkGenerator);

            worldGenerator.ChunkGenerated += (s, e) =>
            {
                Console.WriteLine($"Generated chunk {e.Chunk.X}, {e.Chunk.Y}, {e.Chunk.Z}");
            };

            worldGenerator.CreateChunks();

            Console.WriteLine("Generated world");
        }

        public void Run(int port)
        {
            clientManager = new ClientManager(port);

            clientManager.Error += (s, e) =>
            {
                var error = e.Error;
                Console.WriteLine($"Error: {error}");
            };

            clientManager.Connected += (s, e) =>
            {
                var id = e.Connection.ID;
                Console.WriteLine($"Client connected: {id}");
            };

            clientManager.Disconnected += (s, e) =>
            {
                var id = e.Connection.ID;
                Console.WriteLine($"Client disconnected: {id}");
            };

            clientManager.ChunksRequestReceived += (s, e) =>
            {
                for (var i = 0; i < World.Length; ++i)
                {
                    for (var j = 0; j < World.Width; ++j)
                    {
                        for (var k = 0; k < World.Height; ++k)
                        {
                            var chunk = World.GetChunk(i, j, k);
                            if (chunk != null)
                            {
                                e.Connection.SendChunk(chunk);
                            }
                        }
                    }
                }
            };

            clientManager.Run();

            Console.WriteLine("Listening on the port {0}", port);

            const int updateRate = 60;
            var sw = new Stopwatch();

            while (true)
            {
                sw.Restart();
                clientManager.ProcessMessages();
                World.Update(1f / updateRate);
                sw.Stop();

                var delay = (int)(1000f / updateRate - sw.ElapsedMilliseconds);
                if (delay >= 0)
                {
                    Thread.Sleep(delay);
                }
            }
        }

        public void Dispose() { }
    }
}
