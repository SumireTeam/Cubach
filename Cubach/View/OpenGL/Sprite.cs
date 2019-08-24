using OpenTK;
using OpenTK.Graphics;

namespace Cubach.View.OpenGL
{
    public struct Sprite
    {
        public readonly Vector2 Position;
        public readonly Vector2 Size;
        public readonly Texture Texture;
        public readonly Color4 Color;
        public readonly Vector2 UVMin;
        public readonly Vector2 UVMax;

        public Sprite(Vector2 position, Vector2 size, Texture texture, Color4 color, Vector2 uvMin, Vector2 uvMax)
        {
            Position = position;
            Size = size;
            Texture = texture;
            Color = color;
            UVMax = uvMax;
            UVMin = uvMin;
        }

        public Sprite(Vector2 position, Vector2 size, Texture texture, Color4 color) : this(position, size, texture, color, Vector2.Zero, Vector2.One) { }

        public Sprite(Vector2 position, Vector2 size, Texture texture) : this(position, size, texture, Color4.White, Vector2.Zero, Vector2.One) { }

        public Sprite SetPosition(Vector2 position)
        {
            return new Sprite(position, Size, Texture, Color, UVMin, UVMax);
        }

        public Sprite FlipX()
        {
            Vector2 uvMin = new Vector2(UVMax.X, UVMin.Y);
            Vector2 uvMax = new Vector2(UVMin.X, UVMax.Y);
            return new Sprite(Position, Size, Texture, Color, uvMin, uvMax);
        }

        public Sprite FlipY()
        {
            Vector2 uvMin = new Vector2(UVMin.X, UVMax.Y);
            Vector2 uvMax = new Vector2(UVMax.X, UVMin.Y);
            return new Sprite(Position, Size, Texture, Color, uvMin, uvMax);
        }
    }
}
