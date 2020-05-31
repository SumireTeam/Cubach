using OpenTK;

namespace Cubach.Shared.Math
{
    public interface INoiseProvider
    {
        float Noise(Vector2 position);
        float Noise(Vector3 position);
    }
}
