using OpenTK;

namespace Cubach.View
{
    public struct TextureRegion<TTexture> where TTexture : ITexture
    {
        public readonly TTexture Texture;
        public readonly Vector2 UVMin;
        public readonly Vector2 UVMax;

        public TextureRegion(TTexture texture, Vector2 uvMin, Vector2 uvMax)
        {
            Texture = texture;
            UVMin = uvMin;
            UVMax = uvMax;
        }
    }
}
