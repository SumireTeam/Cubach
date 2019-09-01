using Cubach.Model;
using OpenTK;
using System.Collections.Generic;

namespace Cubach.View
{
    public class ChunkRenderer
    {
        private readonly IMeshFactory meshFactory;
        private readonly ITextureAtlas blockTextureAtlas;

        public ChunkRenderer(IMeshFactory meshFactory, ITextureAtlas blockTextureAtlas)
        {
            this.meshFactory = meshFactory;
            this.blockTextureAtlas = blockTextureAtlas;
        }

        private VertexP3N3T2[] GetChunkVertexes(Chunk chunk)
        {
            // Non-empty natural generated chunks have an average of 50k vertexes.
            // Use the next power of two for the initial capacity to avoid unnecessary allocations.
            var vertexes = new List<VertexP3N3T2>(65536);
            for (var i = 0; i < Chunk.Length; ++i) {
                for (var j = 0; j < Chunk.Width; ++j) {
                    for (var k = 0; k < Chunk.Height; ++k) {
                        var block = chunk.Blocks[i, j, k];
                        if (block.Transparent) {
                            continue;
                        }

                        var rearVisible = i == 0 || i > 0 && chunk.Blocks[i - 1, j, k].Transparent;
                        if (rearVisible) {
                            var normal = -Vector3.UnitX;
                            var textureName = block.Textures.Rear;
                            var textureRegion = blockTextureAtlas.GetRegion(textureName);
                            var uvZero = textureRegion.UVMin;
                            var uvOne = textureRegion.UVMax;
                            var uvUnitX = new Vector2(uvOne.X, uvZero.Y);
                            var uvUnitY = new Vector2(uvZero.X, uvOne.Y);
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, j, k), normal, uvZero));
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, j, 1 + k), normal, uvUnitY));
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, 1 + j, 1 + k), normal, uvOne));
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, j, k), normal, uvZero));
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, 1 + j, 1 + k), normal, uvOne));
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, 1 + j, k), normal, uvUnitX));
                        }

                        var frontVisible = i == Chunk.Length - 1
                                           || i < Chunk.Length - 1 && chunk.Blocks[i + 1, j, k].Transparent;
                        if (frontVisible) {
                            var normal = Vector3.UnitX;
                            var textureName = block.Textures.Front;
                            var textureRegion = blockTextureAtlas.GetRegion(textureName);
                            var uvZero = textureRegion.UVMin;
                            var uvOne = textureRegion.UVMax;
                            var uvUnitX = new Vector2(uvOne.X, uvZero.Y);
                            var uvUnitY = new Vector2(uvZero.X, uvOne.Y);
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, j, k), normal, uvZero));
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, 1 + j, 1 + k), normal, uvOne));
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, j, 1 + k), normal, uvUnitY));
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, j, k), normal, uvZero));
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, 1 + j, k), normal, uvUnitX));
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, 1 + j, 1 + k), normal, uvOne));
                        }

                        var leftVisible = j == 0 || j > 0 && chunk.Blocks[i, j - 1, k].Transparent;
                        if (leftVisible) {
                            var normal = -Vector3.UnitY;
                            var textureName = block.Textures.Left;
                            var textureRegion = blockTextureAtlas.GetRegion(textureName);
                            var uvZero = textureRegion.UVMin;
                            var uvOne = textureRegion.UVMax;
                            var uvUnitX = new Vector2(uvOne.X, uvZero.Y);
                            var uvUnitY = new Vector2(uvZero.X, uvOne.Y);
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, j, k), normal, uvZero));
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, j, 1 + k), normal, uvOne));
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, j, 1 + k), normal, uvUnitY));
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, j, k), normal, uvZero));
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, j, k), normal, uvUnitX));
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, j, 1 + k), normal, uvOne));
                        }

                        var rightVisible = j == Chunk.Width - 1
                                           || j < Chunk.Width - 1 && chunk.Blocks[i, j + 1, k].Transparent;
                        if (rightVisible) {
                            var normal = Vector3.UnitY;
                            var textureName = block.Textures.Right;
                            var textureRegion = blockTextureAtlas.GetRegion(textureName);
                            var uvZero = textureRegion.UVMin;
                            var uvOne = textureRegion.UVMax;
                            var uvUnitX = new Vector2(uvOne.X, uvZero.Y);
                            var uvUnitY = new Vector2(uvZero.X, uvOne.Y);
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, 1 + j, k), normal, uvZero));
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, 1 + j, 1 + k), normal, uvUnitY));
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, 1 + j, 1 + k), normal, uvOne));
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, 1 + j, k), normal, uvZero));
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, 1 + j, 1 + k), normal, uvOne));
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, 1 + j, k), normal, uvUnitX));
                        }

                        var bottomVisible = k == 0 || k > 0 && chunk.Blocks[i, j, k - 1].Transparent;
                        if (bottomVisible) {
                            var normal = -Vector3.UnitZ;
                            var textureName = block.Textures.Bottom;
                            var textureRegion = blockTextureAtlas.GetRegion(textureName);
                            var uvZero = textureRegion.UVMin;
                            var uvOne = textureRegion.UVMax;
                            var uvUnitX = new Vector2(uvOne.X, uvZero.Y);
                            var uvUnitY = new Vector2(uvZero.X, uvOne.Y);
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, j, k), normal, uvZero));
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, 1 + j, k), normal, uvUnitY));
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, 1 + j, k), normal, uvOne));
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, j, k), normal, uvZero));
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, 1 + j, k), normal, uvOne));
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, j, k), normal, uvUnitX));
                        }

                        var topVisible = k == Chunk.Height - 1
                                         || k < Chunk.Height - 1 && chunk.Blocks[i, j, k + 1].Transparent;
                        if (topVisible) {
                            var normal = Vector3.UnitZ;
                            var textureName = block.Textures.Top;
                            var textureRegion = blockTextureAtlas.GetRegion(textureName);
                            var uvZero = textureRegion.UVMin;
                            var uvOne = textureRegion.UVMax;
                            var uvUnitX = new Vector2(uvOne.X, uvZero.Y);
                            var uvUnitY = new Vector2(uvZero.X, uvOne.Y);
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, j, 1 + k), normal, uvZero));
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, 1 + j, 1 + k), normal, uvOne));
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, 1 + j, 1 + k), normal, uvUnitY));
                            vertexes.Add(new VertexP3N3T2(new Vector3(i, j, 1 + k), normal, uvZero));
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, j, 1 + k), normal, uvUnitX));
                            vertexes.Add(new VertexP3N3T2(new Vector3(1 + i, 1 + j, 1 + k), normal, uvOne));
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
