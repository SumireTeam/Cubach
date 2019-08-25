using OpenTK;
using OpenTK.Graphics;

namespace Cubach.View
{
    public struct Sprite<TTexture> where TTexture : ITexture
    {
        public readonly Vector2 Position;
        public readonly Vector2 Size;
        public readonly TextureRegion<TTexture> TextureRegion;
        public readonly Color4 Color;

        public Sprite(Vector2 position, Vector2 size, TextureRegion<TTexture> textureRegion, Color4 color)
        {
            Position = position;
            Size = size;
            TextureRegion = textureRegion;
            Color = color;
        }

        public Sprite(Vector2 position, Vector2 size, TextureRegion<TTexture> textureRegion) : this(position, size, textureRegion, Color4.White) { }

        public Sprite<TTexture> SetPosition(Vector2 position) => new Sprite<TTexture>(position, Size, TextureRegion, Color);
        public Sprite<TTexture> SetSize(Vector2 size) => new Sprite<TTexture>(Position, size, TextureRegion, Color);

        public Sprite<TTexture> FlipX()
        {
            var uvMin = new Vector2(TextureRegion.UVMax.X, TextureRegion.UVMin.Y);
            var uvMax = new Vector2(TextureRegion.UVMin.X, TextureRegion.UVMax.Y);
            var textureRegion = new TextureRegion<TTexture>(TextureRegion.Texture, uvMin, uvMax);
            return new Sprite<TTexture>(Position, Size, textureRegion, Color);
        }

        public Sprite<TTexture> FlipY()
        {
            var uvMin = new Vector2(TextureRegion.UVMin.X, TextureRegion.UVMax.Y);
            var uvMax = new Vector2(TextureRegion.UVMax.X, TextureRegion.UVMin.Y);
            var textureRegion = new TextureRegion<TTexture>(TextureRegion.Texture, uvMin, uvMax);
            return new Sprite<TTexture>(Position, Size, textureRegion, Color);
        }
    }
}
