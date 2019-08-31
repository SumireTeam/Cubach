using System;
using Cubach.Model;
using OpenTK;
using System.Collections.Generic;

namespace Cubach.View
{
    public class ChunkRenderer
    {
        private readonly IMeshFactory meshFactory;

        public ChunkRenderer(IMeshFactory meshFactory) => this.meshFactory = meshFactory;

        private VertexP3N3T2[] GetChunkVertexes(Chunk chunk)
        {
            var vertexes = new List<VertexP3N3T2>();
            for (int i = 0; i < Chunk.Length; ++i)
            {
                for (int j = 0; j < Chunk.Width; ++j)
                {
                    for (int k = 0; k < Chunk.Height; ++k)
                    {
                        Block block = chunk.Blocks[i, j, k];
                        if (block.Transparent)
                        {
                            continue;
                        }

                        bool rearVisible = i == 0 || i > 0 && chunk.Blocks[i - 1, j, k].Transparent;
                        if (rearVisible)
                        {
                            vertexes.AddRange(new VertexP3N3T2[] {
                                new VertexP3N3T2(new Vector3(i,     j,     k), -Vector3.UnitX, new Vector2(0, 0)),
                                new VertexP3N3T2(new Vector3(i,     j, 1 + k), -Vector3.UnitX, new Vector2(0, 1)),
                                new VertexP3N3T2(new Vector3(i, 1 + j, 1 + k), -Vector3.UnitX, new Vector2(1, 1)),

                                new VertexP3N3T2(new Vector3(i,     j,     k), -Vector3.UnitX, new Vector2(0, 0)),
                                new VertexP3N3T2(new Vector3(i, 1 + j, 1 + k), -Vector3.UnitX, new Vector2(1, 1)),
                                new VertexP3N3T2(new Vector3(i, 1 + j,     k), -Vector3.UnitX, new Vector2(1, 0)),
                            });
                        }

                        bool frontVisible = i == Chunk.Length - 1 || i < Chunk.Length - 1 && chunk.Blocks[i + 1, j, k].Transparent;
                        if (frontVisible)
                        {
                            vertexes.AddRange(new VertexP3N3T2[] {
                                new VertexP3N3T2(new Vector3(1 + i,     j,     k), Vector3.UnitX, new Vector2(0, 0)),
                                new VertexP3N3T2(new Vector3(1 + i, 1 + j, 1 + k), Vector3.UnitX, new Vector2(1, 1)),
                                new VertexP3N3T2(new Vector3(1 + i,     j, 1 + k), Vector3.UnitX, new Vector2(0, 1)),

                                new VertexP3N3T2(new Vector3(1 + i,     j,     k), Vector3.UnitX, new Vector2(0, 0)),
                                new VertexP3N3T2(new Vector3(1 + i, 1 + j,     k), Vector3.UnitX, new Vector2(1, 0)),
                                new VertexP3N3T2(new Vector3(1 + i, 1 + j, 1 + k), Vector3.UnitX, new Vector2(1, 1)),
                            });
                        }

                        bool leftVisible = j == 0 || j > 0 && chunk.Blocks[i, j - 1, k].Transparent;
                        if (leftVisible)
                        {
                            vertexes.AddRange(new VertexP3N3T2[] {
                                new VertexP3N3T2(new Vector3(    i, j,     k), -Vector3.UnitY, new Vector2(0, 0)),
                                new VertexP3N3T2(new Vector3(1 + i, j, 1 + k), -Vector3.UnitY, new Vector2(1, 1)),
                                new VertexP3N3T2(new Vector3(    i, j, 1 + k), -Vector3.UnitY, new Vector2(0, 1)),

                                new VertexP3N3T2(new Vector3(    i, j,     k), -Vector3.UnitY, new Vector2(0, 0)),
                                new VertexP3N3T2(new Vector3(1 + i, j,     k), -Vector3.UnitY, new Vector2(1, 0)),
                                new VertexP3N3T2(new Vector3(1 + i, j, 1 + k), -Vector3.UnitY, new Vector2(1, 1)),
                            });
                        }

                        bool rightVisible = j == Chunk.Width - 1 || j < Chunk.Width - 1 && chunk.Blocks[i, j + 1, k].Transparent;
                        if (rightVisible)
                        {
                            vertexes.AddRange(new VertexP3N3T2[] {
                                new VertexP3N3T2(new Vector3(    i, 1 + j,     k), Vector3.UnitY, new Vector2(0, 0)),
                                new VertexP3N3T2(new Vector3(    i, 1 + j, 1 + k), Vector3.UnitY, new Vector2(0, 1)),
                                new VertexP3N3T2(new Vector3(1 + i, 1 + j, 1 + k), Vector3.UnitY, new Vector2(1, 1)),

                                new VertexP3N3T2(new Vector3(    i, 1 + j,     k), Vector3.UnitY, new Vector2(0, 0)),
                                new VertexP3N3T2(new Vector3(1 + i, 1 + j, 1 + k), Vector3.UnitY, new Vector2(1, 1)),
                                new VertexP3N3T2(new Vector3(1 + i, 1 + j,     k), Vector3.UnitY, new Vector2(1, 0)),
                            });
                        }

                        bool bottomVisible = k == 0 || k > 0 && chunk.Blocks[i, j, k - 1].Transparent;
                        if (bottomVisible)
                        {
                            vertexes.AddRange(new VertexP3N3T2[] {
                                new VertexP3N3T2(new Vector3(    i,     j, k), -Vector3.UnitZ, new Vector2(0, 0)),
                                new VertexP3N3T2(new Vector3(    i, 1 + j, k), -Vector3.UnitZ, new Vector2(0, 1)),
                                new VertexP3N3T2(new Vector3(1 + i, 1 + j, k), -Vector3.UnitZ, new Vector2(1, 1)),

                                new VertexP3N3T2(new Vector3(    i,     j, k), -Vector3.UnitZ, new Vector2(0, 0)),
                                new VertexP3N3T2(new Vector3(1 + i, 1 + j, k), -Vector3.UnitZ, new Vector2(1, 1)),
                                new VertexP3N3T2(new Vector3(1 + i,     j, k), -Vector3.UnitZ, new Vector2(1, 0)),
                            });
                        }

                        bool topVisible = k == Chunk.Height - 1 || k < Chunk.Height - 1 && chunk.Blocks[i, j, k + 1].Transparent;
                        if (topVisible)
                        {
                            vertexes.AddRange(new VertexP3N3T2[] {
                                new VertexP3N3T2(new Vector3(    i,     j, 1 + k), Vector3.UnitZ, new Vector2(0, 0)),
                                new VertexP3N3T2(new Vector3(1 + i, 1 + j, 1 + k), Vector3.UnitZ, new Vector2(1, 1)),
                                new VertexP3N3T2(new Vector3(    i, 1 + j, 1 + k), Vector3.UnitZ, new Vector2(0, 1)),

                                new VertexP3N3T2(new Vector3(    i,     j, 1 + k), Vector3.UnitZ, new Vector2(0, 0)),
                                new VertexP3N3T2(new Vector3(1 + i,     j, 1 + k), Vector3.UnitZ, new Vector2(1, 0)),
                                new VertexP3N3T2(new Vector3(1 + i, 1 + j, 1 + k), Vector3.UnitZ, new Vector2(1, 1)),
                            });
                        }
                    }
                }
            }

            return vertexes.ToArray();
        }

        public IMesh<VertexP3N3T2> CreateChunkMesh(Chunk chunk)
        {
            var vertexes = GetChunkVertexes(chunk);
            return meshFactory.Create(vertexes);
        }

        public void UpdateChunkMesh(Chunk chunk, IMesh<VertexP3N3T2> mesh)
        {
            var vertexes = GetChunkVertexes(chunk);
            mesh.SetData(vertexes);
        }
    }
}
