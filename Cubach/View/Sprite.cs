using OpenTK;
using OpenTK.Graphics;

namespace Cubach.View
{
    public struct Sprite<TTexture> where TTexture : ITexture
    {
        public readonly Vector2 Position;
        public readonly Vector2 Size;
        public readonly TTexture Texture;
        public readonly Color4 Color;
        public readonly Vector2 UVMin;
        public readonly Vector2 UVMax;

        public Sprite(Vector2 position, Vector2 size, TTexture texture, Color4 color, Vector2 uvMin, Vector2 uvMax)
        {
            Position = position;
            Size = size;
            Texture = texture;
            Color = color;
            UVMax = uvMax;
            UVMin = uvMin;
        }

        public Sprite(Vector2 position, Vector2 size, TTexture texture, Color4 color) : this(position, size, texture, color, Vector2.Zero, Vector2.One) { }

        public Sprite(Vector2 position, Vector2 size, TTexture texture) : this(position, size, texture, Color4.White, Vector2.Zero, Vector2.One) { }

        public Sprite<TTexture> SetPosition(Vector2 position)
        {
            return new Sprite<TTexture>(position, Size, Texture, Color, UVMin, UVMax);
        }

        public Sprite<TTexture> FlipX()
        {
            Vector2 uvMin = new Vector2(UVMax.X, UVMin.Y);
            Vector2 uvMax = new Vector2(UVMin.X, UVMax.Y);
            return new Sprite<TTexture>(Position, Size, Texture, Color, uvMin, uvMax);
        }

        public Sprite<TTexture> FlipY()
        {
            Vector2 uvMin = new Vector2(UVMin.X, UVMax.Y);
            Vector2 uvMax = new Vector2(UVMax.X, UVMin.Y);
            return new Sprite<TTexture>(Position, Size, Texture, Color, uvMin, uvMax);
        }
    }
}
