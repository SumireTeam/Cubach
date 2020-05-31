using System;
using System.Collections.Generic;
using System.Linq;
using Cubach.Shared;
using Cubach.Shared.Math;
using Cubach.View.OpenGL;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Configuration = Cubach.Shared.Configuration;

namespace Cubach.View
{
    public class WorldRenderer : IDisposable
    {
        private const int MaxChunkUpdatesPerRender = 2;

        private readonly Configuration config;
        private readonly World world;
        private readonly ICamera camera;
        private readonly ChunkRenderer chunkRenderer;

        private readonly bool[,,] chunkRequiresUpdate = new bool[World.Length, World.Width, World.Height];

        private readonly IMesh<VertexP3N3T2>[,,] chunkMeshes =
            new IMesh<VertexP3N3T2>[World.Length, World.Width, World.Height];

        private readonly IMesh<VertexP3N3T2>[,,] chunkSemiTransparentMeshes =
            new IMesh<VertexP3N3T2>[World.Length, World.Width, World.Height];

        public WorldRenderer(Configuration config, World world, ICamera camera, ChunkRenderer chunkRenderer)
        {
            this.config = config;
            this.world = world;
            this.camera = camera;
            this.chunkRenderer = chunkRenderer;

            for (var i = 0; i < World.Length; ++i)
            {
                for (var j = 0; j < World.Width; ++j)
                {
                    for (var k = 0; k < World.Height; ++k)
                    {
                        var chunk = world.GetChunk(i, j, k);
                        chunkMeshes[i, j, k] = chunkRenderer.CreateChunkMesh(world, chunk, BlockTransparency.Opaque);
                        chunkSemiTransparentMeshes[i, j, k] =
                            chunkRenderer.CreateChunkMesh(world, chunk, BlockTransparency.SemiTransparent);
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
            var chunk = world.GetChunk(x, y, z);

            var mesh = chunkMeshes[x, y, z];
            chunkRenderer.UpdateChunkMesh(world, chunk, BlockTransparency.Opaque, mesh);

            var semiTransparentMesh = chunkSemiTransparentMeshes[x, y, z];
            chunkRenderer.UpdateChunkMesh(world, chunk, BlockTransparency.SemiTransparent, semiTransparentMesh);
        }

        public void Draw(ShaderProgram shader, float aspect, float time)
        {
            shader.Use();

            var chunkUpdates = 0;
            var playerDir = camera.Front;

            var view = camera.View;
            var projection = camera.Projection;
            var vp = view * projection;

            // Sort chunks by distance, so near chunks are rendered first,
            // and rendering of the far chunks can be skipped with the depth test.
            var chunks = new SortedList<float, List<Chunk>>();
            for (var i = 0; i < World.Length; ++i)
            {
                for (var j = 0; j < World.Width; ++j)
                {
                    for (var k = 0; k < World.Height; ++k)
                    {
                        // Skip empty chunks that not requires update.
                        var chunkMesh = chunkMeshes[i, j, k];
                        var chunkSemiTransparentMesh = chunkSemiTransparentMeshes[i, j, k];
                        var requiresUpdate = chunkRequiresUpdate[i, j, k];
                        if (chunkMesh.VertexCount == 0
                            && chunkSemiTransparentMesh.VertexCount == 0 && !requiresUpdate)
                        {
                            continue;
                        }

                        var chunk = world.GetChunk(i, j, k);
                        var chunkCenter = chunk.Center;
                        var distance = MathUtils.TaxicabDistance(chunkCenter, camera.Position);

                        // Skip chunk if it is too far.
                        if (distance > config.RenderDistance)
                        {
                            continue;
                        }

                        // Skip chunk if it is not current and behind the player.
                        if (distance > Chunk.Length + Chunk.Width + Chunk.Height)
                        {
                            var direction = (chunkCenter - camera.Position).Normalized();
                            if (Vector3.Dot(playerDir, direction) < 0)
                            {
                                continue;
                            }
                        }

                        if (chunks.ContainsKey(distance))
                        {
                            chunks[distance].Add(chunk);
                        }
                        else
                        {
                            chunks.Add(distance, new List<Chunk> { chunk });
                        }
                    }
                }
            }

            GL.Disable(EnableCap.Blend);

            foreach (var chunk in chunks.SelectMany(kv => kv.Value))
            {
                // Update chunk mesh if required.
                if (chunkUpdates < MaxChunkUpdatesPerRender && chunkRequiresUpdate[chunk.X, chunk.Y, chunk.Z])
                {
                    UpdateChunk(chunk.X, chunk.Y, chunk.Z);
                    chunkRequiresUpdate[chunk.X, chunk.Y, chunk.Z] = false;
                    chunkUpdates++;
                }

                // Draw chunk mesh.
                var model = Matrix4.CreateTranslation(chunk.Min);
                var mvp = model * vp;
                shader.SetUniform("mvp", ref mvp);
                chunkMeshes[chunk.X, chunk.Y, chunk.Z].Draw();
            }

            GL.Enable(EnableCap.Blend);

            foreach (var chunk in chunks.SelectMany(kv => kv.Value))
            {
                // Draw chunk semi-transparent mesh.
                var model = Matrix4.CreateTranslation(chunk.Min);
                var mvp = model * vp;
                shader.SetUniform("mvp", ref mvp);
                chunkSemiTransparentMeshes[chunk.X, chunk.Y, chunk.Z].Draw();
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < World.Length; ++i)
            {
                for (int j = 0; j < World.Width; ++j)
                {
                    for (int k = 0; k < World.Height; ++k)
                    {
                        chunkMeshes[i, j, k]?.Dispose();
                        chunkSemiTransparentMeshes[i, j, k]?.Dispose();
                    }
                }
            }
        }
    }
}
