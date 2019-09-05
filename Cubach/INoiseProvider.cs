using OpenTK;

namespace Cubach
{
    public interface INoiseProvider
    {
        float Noise(Vector2 position);
        float Noise(Vector3 position);
    }
}
