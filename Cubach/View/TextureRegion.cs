using OpenTK;

namespace Cubach.View
{
    public struct TextureRegion
    {
        public readonly Vector2 UVMin;
        public readonly Vector2 UVMax;

        public TextureRegion(Vector2 uvMin, Vector2 uvMax)
        {
            UVMin = uvMin;
            UVMax = uvMax;
        }
    }
}
