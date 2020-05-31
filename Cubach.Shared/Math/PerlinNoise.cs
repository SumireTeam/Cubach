using OpenTK;
using SMath = System.Math;

namespace Cubach.Shared.Math
{
    public class PerlinNoise : INoiseProvider
    {
        private readonly IRandomProvider randomProvider;

        public PerlinNoise(IRandomProvider randomProvider) => this.randomProvider = randomProvider;

        private Vector2 RandomUnit(int x, int y)
        {
            int t = randomProvider.Random(x, y);
            return MathUtils.PolarToCartesian(1, t);
        }

        private Vector3 RandomUnit(int x, int y, int z)
        {
            int t = randomProvider.Random(x, y, z);
            return MathUtils.SphericalToCartesian(1, t, t);
        }

        public float Noise(Vector2 position)
        {
            var p = new Vector2((int)SMath.Floor(position.X), (int)SMath.Floor(position.Y));
            var d = position - p;
            var s = new float[2, 2];
            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    var gradient = RandomUnit((int)p.X + i, (int)p.Y + j);
                    var distance = new Vector2(d.X - i, d.Y - j);
                    s[i, j] = Vector2.Dot(gradient, distance);
                }
            }

            var t = MathUtils.SCurve(d);
            return MathUtils.BilinearInterpolation(new Quad<float>(s), t);
        }

        public float Noise(Vector3 position)
        {
            var p = new Vector3(
                (int)SMath.Floor(position.X),
                (int)SMath.Floor(position.Y),
                (int)SMath.Floor(position.Z)
            );
            var d = position - p;
            var s = new float[2, 2, 2];
            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    for (int k = 0; k < 2; ++k)
                    {
                        var gradient = RandomUnit((int)p.X + i, (int)p.Y + j, (int)p.Z + k);
                        var distance = new Vector3(d.X - i, d.Y - j, d.Z - k);
                        s[i, j, k] = Vector3.Dot(gradient, distance);
                    }
                }
            }

            var t = MathUtils.SCurve(d);
            return MathUtils.TrilinearInterpolation(new Cube<float>(s), t);
        }
    }
}
