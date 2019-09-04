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

        private VertexP3N3T2[] GetChunkVertexes(World world, Chunk chunk)
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

                        var rearBlock = world.GetBlockAt(chunk.Min + new Vector3(i - 1, j, k));
                        var rearVisible = !rearBlock.HasValue || rearBlock.Value.Transparent;
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

                        var frontBlock = world.GetBlockAt(chunk.Min + new Vector3(i + 1, j, k));
                        var frontVisible = !frontBlock.HasValue || frontBlock.Value.Transparent;
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

                        var leftBlock = world.GetBlockAt(chunk.Min + new Vector3(i, j - 1, k));
                        var leftVisible = !leftBlock.HasValue || leftBlock.Value.Transparent;
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

                        var rightBlock = world.GetBlockAt(chunk.Min + new Vector3(i, j + 1, k));
                        var rightVisible = !rightBlock.HasValue || rightBlock.Value.Transparent;
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

                        var bottomBlock = world.GetBlockAt(chunk.Min + new Vector3(i, j, k - 1));
                        var bottomVisible = !bottomBlock.HasValue || bottomBlock.Value.Transparent;
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

                        var topBlock = world.GetBlockAt(chunk.Min + new Vector3(i, j, k + 1));
                        var topVisible = !topBlock.HasValue || topBlock.Value.Transparent;
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

        public IMesh<VertexP3N3T2> CreateChunkMesh(World world, Chunk chunk)
        {
            var vertexes = GetChunkVertexes(world, chunk);
            return meshFactory.Create(vertexes);
        }

        public void UpdateChunkMesh(World world, Chunk chunk, IMesh<VertexP3N3T2> mesh)
        {
            var vertexes = GetChunkVertexes(world, chunk);
            mesh.SetData(vertexes);
        }
    }
}
