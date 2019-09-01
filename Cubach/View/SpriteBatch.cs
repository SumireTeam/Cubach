using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Cubach.View
{
    public class SpriteBatch<TTexture> : IDisposable where TTexture : ITexture
    {
        private readonly List<Sprite<TTexture>> sprites = new List<Sprite<TTexture>>();
        private readonly IMesh<VertexP2T2C4> mesh;

        public SpriteBatch(IMeshFactory meshFactory) =>
            mesh = meshFactory.Create(new VertexP2T2C4[0], MeshUsageHint.Dynamic);

        public void Begin() => sprites.Clear();

        public void Draw(Sprite<TTexture> sprite) => sprites.Add(sprite);

        public void End()
        {
            var groups = sprites.GroupBy(sprite => sprite.Texture);
            foreach (var group in groups) {
                int spriteCount = group.Count();
                int vertexCount = 6 * spriteCount;
                VertexP2T2C4[] vertexes = new VertexP2T2C4[vertexCount];
                int index = 0;
                foreach (var sprite in group) {
                    Vector2 p1 = sprite.Position;
                    Vector2 p2 = new Vector2(sprite.Position.X, sprite.Position.Y + sprite.Size.Y);
                    Vector2 p3 = sprite.Position + sprite.Size;
                    Vector2 p4 = new Vector2(sprite.Position.X + sprite.Size.X, sprite.Position.Y);

                    Vector2 t1 = new Vector2(sprite.TextureRegion.UVMin.X, sprite.TextureRegion.UVMax.Y);
                    Vector2 t2 = sprite.TextureRegion.UVMin;
                    Vector2 t3 = new Vector2(sprite.TextureRegion.UVMax.X, sprite.TextureRegion.UVMin.Y);
                    Vector2 t4 = sprite.TextureRegion.UVMax;

                    Vector4 color = new Vector4(sprite.Color.R, sprite.Color.G, sprite.Color.B, sprite.Color.A);

                    vertexes[index++] = new VertexP2T2C4(p1, t1, color);
                    vertexes[index++] = new VertexP2T2C4(p2, t2, color);
                    vertexes[index++] = new VertexP2T2C4(p3, t3, color);

                    vertexes[index++] = new VertexP2T2C4(p1, t1, color);
                    vertexes[index++] = new VertexP2T2C4(p3, t3, color);
                    vertexes[index++] = new VertexP2T2C4(p4, t4, color);
                }

                mesh.SetData(vertexes, MeshUsageHint.Dynamic);

                TTexture texture = group.Key;
                texture.Bind();
                mesh.Draw();
            }

            sprites.Clear();
        }

        public void Dispose() => mesh.Dispose();
    }
}
