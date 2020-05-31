using OpenTK;
using SMath = System.Math;

namespace Cubach.Shared.Math
{
    public static class MathUtils
    {
        public static float SCurve(float t) => t * t * (3f - 2f * t);
        public static Vector2 SCurve(Vector2 t) => new Vector2(SCurve(t.X), SCurve(t.Y));
        public static Vector3 SCurve(Vector3 t) => new Vector3(SCurve(t.X), SCurve(t.Y), SCurve(t.Z));

        public static Vector4 SCurve(Vector4 t) =>
            new Vector4(SCurve(t.X), SCurve(t.Y), SCurve(t.Z), SCurve(t.W));

        public static float LinearInterpolation(float a, float b, float t) => t * (b - a) + a;
        public static Vector2 LinearInterpolation(Vector2 a, Vector2 b, float t) => t * (b - a) + a;
        public static Vector3 LinearInterpolation(Vector3 a, Vector3 b, float t) => t * (b - a) + a;
        public static Vector4 LinearInterpolation(Vector4 a, Vector4 b, float t) => t * (b - a) + a;

        public static float LinearInterpolation(Line<float> line, float t) =>
            LinearInterpolation(line.Start, line.End, t);

        public static Vector2 LinearInterpolation(Line<Vector2> line, float t) =>
            LinearInterpolation(line.Start, line.End, t);

        public static Vector3 LinearInterpolation(Line<Vector3> line, float t) =>
            LinearInterpolation(line.Start, line.End, t);

        public static Vector4 LinearInterpolation(Line<Vector4> line, float t) =>
            LinearInterpolation(line.Start, line.End, t);

        public static float BilinearInterpolation(Quad<float> quad, Vector2 t)
        {
            float a = LinearInterpolation(quad.Bottom, t.X);
            float b = LinearInterpolation(quad.Top, t.X);
            return LinearInterpolation(a, b, t.Y);
        }

        public static float TrilinearInterpolation(Cube<float> cube, Vector3 t)
        {
            float a = BilinearInterpolation(cube.Bottom, t.Xy);
            float b = BilinearInterpolation(cube.Top, t.Xy);
            return LinearInterpolation(a, b, t.Z);
        }

        public static Vector2 PolarToCartesian(float r, float a)
        {
            float x = (float)(r * SMath.Sin(a));
            float y = (float)(r * SMath.Cos(a));
            return new Vector2(x, y);
        }

        public static Vector3 SphericalToCartesian(float r, float inclination, float azimuth)
        {
            double sinI = SMath.Sin(inclination);
            float x = (float)(r * sinI * SMath.Cos(azimuth));
            float y = (float)(r * sinI * SMath.Sin(azimuth));
            float z = (float)(r * SMath.Cos(inclination));
            return new Vector3(x, y, z);
        }

        public static float TaxicabDistance(Vector2 a, Vector2 b) =>
            SMath.Abs(b.X - a.X) + SMath.Abs(b.Y - a.Y);

        public static float TaxicabDistance(Vector3 a, Vector3 b) =>
            SMath.Abs(b.X - a.X) + SMath.Abs(b.Y - a.Y) + SMath.Abs(b.Z - a.Z);
    }
}
