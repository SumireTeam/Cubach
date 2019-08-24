using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Cubach.View.OpenGL
{
    public class SpriteBatch : ISpriteBatch<Texture>, IDisposable
    {
        protected readonly List<Sprite<Texture>> Sprites = new List<Sprite<Texture>>();
        protected readonly VertexArray VertexArray;
        protected readonly VertexBuffer VertexBuffer;

        public SpriteBatch()
        {
            VertexArray = new VertexArray();
            VertexBuffer = new VertexBuffer();

            for (int i = 0; i < VertexP2T2C4.VertexAttributes.Length; ++i)
            {
                VertexArray.SetVertexAttribute(i, VertexP2T2C4.VertexAttributes[i], VertexBuffer);
            }

            VertexArray.Unbind();
        }

        public void Begin()
        {
            Sprites.Clear();
        }

        public void Draw(Sprite<Texture> sprite)
        {
            Sprites.Add(sprite);
        }

        public void End()
        {
            var groups = Sprites.GroupBy(sprite => sprite.Texture);
            foreach (var group in groups)
            {
                int spriteCount = group.Count();
                int vertexCount = 6 * spriteCount;
                VertexP2T2C4[] vertexes = new VertexP2T2C4[vertexCount];
                int index = 0;
                foreach (var sprite in group)
                {
                    Vector2 p1 = sprite.Position;
                    Vector2 p2 = new Vector2(sprite.Position.X, sprite.Position.Y + sprite.Size.Y);
                    Vector2 p3 = sprite.Position + sprite.Size;
                    Vector2 p4 = new Vector2(sprite.Position.X + sprite.Size.X, sprite.Position.Y);

                    Vector2 t1 = new Vector2(sprite.UVMin.X, sprite.UVMax.Y);
                    Vector2 t2 = sprite.UVMin;
                    Vector2 t3 = new Vector2(sprite.UVMax.X, sprite.UVMin.Y);
                    Vector2 t4 = sprite.UVMax;

                    Vector4 color = new Vector4(sprite.Color.R, sprite.Color.G, sprite.Color.B, sprite.Color.A);

                    vertexes[index++] = new VertexP2T2C4(p1, t1, color);
                    vertexes[index++] = new VertexP2T2C4(p2, t2, color);
                    vertexes[index++] = new VertexP2T2C4(p3, t3, color);

                    vertexes[index++] = new VertexP2T2C4(p1, t1, color);
                    vertexes[index++] = new VertexP2T2C4(p3, t3, color);
                    vertexes[index++] = new VertexP2T2C4(p4, t4, color);
                }

                VertexBuffer.SetData(vertexes, BufferUsageHint.DynamicDraw);

                Texture texture = group.Key;
                Texture.Bind(texture);

                VertexArray.Draw(PrimitiveType.Triangles, 0, vertexCount);
                VertexArray.Unbind();
            }

            Sprites.Clear();
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            VertexArray.Dispose();
        }
    }
}
