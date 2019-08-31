using System;
using Cubach.Model;
using Cubach.View.OpenGL;
using OpenTK;

namespace Cubach.View
{
    public class WorldRenderer : IDisposable
    {
        private const int MaxChunkUpdatesPerRender = 10;

        private readonly World world;
        private readonly ChunkRenderer chunkRenderer;

        private readonly bool[,,] chunkRequiresUpdate = new bool[World.Length, World.Width, World.Height];

        private readonly IMesh<VertexP3N3T2>[,,] chunkMeshes =
            new IMesh<VertexP3N3T2>[World.Length, World.Width, World.Height];

        private float azimuth = 0;

        public WorldRenderer(World world, ChunkRenderer chunkRenderer)
        {
            this.world = world;
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

            const float fovx = MathHelper.PiOver2;
            const float fovEps = 0.1f;
            float fovy = Math.Min(Math.Max(fovx / aspect, fovEps), MathHelper.Pi - fovEps);

            azimuth += time * MathHelper.DegreesToRadians(30);

            var target = new Vector3(
                Chunk.Length * World.Length / 2f,
                Chunk.Width * World.Width / 2f,
                Chunk.Height * World.Height / 2f
            );

            var position = target + Matrix3.CreateRotationZ(azimuth) *
                           new Vector3(1.5f * Chunk.Length * World.Length, 0.0f, 32);

            var view = Matrix4.LookAt(position, target, Vector3.UnitZ);
            var projection = Matrix4.CreatePerspectiveFieldOfView(fovy, aspect, 0.1f, 1024);

            var chunkUpdates = 0;

            for (int i = 0; i < World.Length; ++i) {
                for (int j = 0; j < World.Width; ++j) {
                    for (int k = 0; k < World.Height; ++k) {
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
