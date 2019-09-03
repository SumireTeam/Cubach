using System;
using Cubach.Model;
using Cubach.View.OpenGL;
using OpenTK;

namespace Cubach.View
{
    public class WorldRenderer : IDisposable
    {
        private const int MaxChunkUpdatesPerRender = 20;

        private readonly World world;
        private readonly ICamera camera;
        private readonly ChunkRenderer chunkRenderer;

        private readonly bool[,,] chunkRequiresUpdate = new bool[World.Length, World.Width, World.Height];

        private readonly IMesh<VertexP3N3T2>[,,] chunkMeshes =
            new IMesh<VertexP3N3T2>[World.Length, World.Width, World.Height];

        public WorldRenderer(World world, ICamera camera, ChunkRenderer chunkRenderer)
        {
            this.world = world;
            this.camera = camera;
            this.chunkRenderer = chunkRenderer;

            for (int i = 0; i < World.Length; ++i) {
                for (int j = 0; j < World.Width; ++j) {
                    for (int k = 0; k < World.Height; ++k) {
                        var chunk = world.Chunks[i, j, k];
                        chunkMeshes[i, j, k] = chunkRenderer.CreateChunkMesh(chunk);
                    }
                }
            }
        }

        public void RequestChunkUpdate(int x, int y, int z)
        {
            chunkRequiresUpdate[x, y, z] = true;
        }

        private void UpdateChunk(int x, int y, int z)
        {
            var chunk = world.Chunks[x, y, z];
            var mesh = chunkMeshes[x, y, z];
            chunkRenderer.UpdateChunkMesh(chunk, mesh);
        }

        public void Draw(ShaderProgram shader, float aspect, float time)
        {
            shader.Use();

            var chunkUpdates = 0;
            var playerDir = camera.Front;

            var view = camera.View;
            var projection = camera.Projection;

            for (var i = 0; i < World.Length; ++i) {
                for (var j = 0; j < World.Width; ++j) {
                    for (var k = 0; k < World.Height; ++k) {
                        var chunkCenter = new Vector3(Chunk.Length * (i + 0.5f),
                            Chunk.Width * (j + 0.5f),
                            Chunk.Height * (k + 0.5f));
                        var playerToChunkDist = MathUtils.TaxicabDistance(chunkCenter, camera.Position);
                        if (playerToChunkDist > (Chunk.Length + Chunk.Width + Chunk.Height) / 2f) {
                            // If the chunk is behind the player, skip it.
                            var playerToChunkDir = (chunkCenter - camera.Position).Normalized();
                            if (Vector3.Dot(playerDir, playerToChunkDir) < 0) {
                                continue;
                            }
                        }

                        var model = Matrix4.CreateTranslation(Chunk.Length * i, Chunk.Width * j, Chunk.Height * k);
                        var mvp = model * view * projection;
                        shader.SetUniform("mvp", ref mvp);

                        if (chunkUpdates < MaxChunkUpdatesPerRender && chunkRequiresUpdate[i, j, k]) {
                            UpdateChunk(i, j, k);
                            chunkRequiresUpdate[i, j, k] = false;
                            chunkUpdates++;
                        }

                        chunkMeshes[i, j, k].Draw();
                    }
                }
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < World.Length; ++i) {
                for (int j = 0; j < World.Width; ++j) {
                    for (int k = 0; k < World.Height; ++k) {
                        chunkMeshes[i, j, k].Dispose();
                    }
                }
            }
        }
    }
}
