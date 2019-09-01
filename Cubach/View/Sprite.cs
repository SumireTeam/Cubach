using OpenTK;
using OpenTK.Graphics;

namespace Cubach.View
{
    public struct Sprite<TTexture> where TTexture : ITexture
    {
        public readonly Vector2 Position;
        public readonly Vector2 Size;
        public readonly TTexture Texture;
        public readonly TextureRegion TextureRegion;
        public readonly Color4 Color;

        public Sprite(Vector2 position, Vector2 size, TTexture texture, TextureRegion textureRegion, Color4 color)
        {
            Position = position;
            Size = size;
            Texture = texture;
            TextureRegion = textureRegion;
            Color = color;
        }

        public Sprite(Vector2 position, Vector2 size, TTexture texture, TextureRegion textureRegion)
            : this(position, size, texture, textureRegion, Color4.White) { }

        public Sprite<TTexture> SetPosition(Vector2 position) =>
            new Sprite<TTexture>(position, Size, Texture, TextureRegion, Color);

        public Sprite<TTexture> SetSize(Vector2 size) =>
            new Sprite<TTexture>(Position, size, Texture, TextureRegion, Color);

        public Sprite<TTexture> FlipX()
        {
            var uvMin = new Vector2(TextureRegion.UVMax.X, TextureRegion.UVMin.Y);
            var uvMax = new Vector2(TextureRegion.UVMin.X, TextureRegion.UVMax.Y);
            var textureRegion = new TextureRegion(uvMin, uvMax);
            return new Sprite<TTexture>(Position, Size, Texture, textureRegion, Color);
        }

        public Sprite<TTexture> FlipY()
        {
            var uvMin = new Vector2(TextureRegion.UVMin.X, TextureRegion.UVMax.Y);
            var uvMax = new Vector2(TextureRegion.UVMax.X, TextureRegion.UVMin.Y);
            var textureRegion = new TextureRegion(uvMin, uvMax);
            return new Sprite<TTexture>(Position, Size, Texture, textureRegion, Color);
        }
    }
}
