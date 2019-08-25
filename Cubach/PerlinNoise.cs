using OpenTK;
using System;

namespace Cubach
{
    public static class PerlinNoise
    {
        public static Vector2 RandomUnit2(int x, int y)
        {
            double t = ((x * 99990001) ^ (y * 999999000001) ^ 333667) / 9901;
            float tx = (float)Math.Sin(t);
            float ty = (float)Math.Cos(t);
            return new Vector2(tx, ty);
        }

        public static Vector3 RandomUnit3(int x, int y, int z)
        {
            double th = ((x * 99990001) ^ (y * 999999000001) ^ (z * 9999999900000001) ^ 333667) / 9901;
            double tv = ((x * 99990001) ^ (y * 999999000001) ^ (z * 9999999900000001) ^ 333667) / 9901;
            float tx = (float)(Math.Sin(tv) * Math.Cos(th));
            float ty = (float)(Math.Sin(tv) * Math.Sin(th));
            float tz = (float)Math.Cos(tv);
            return new Vector3(tx, ty, tz);
        }

        public static float SCurve(float t)
        {
            return t * t * (3f - 2f * t);
        }

        public static float LinearInterpolation(float a, float b, float t)
        {
            return t * (b - a) + a;
        }

        public static float Noise2(Vector2 position)
        {
            int x = (int)Math.Floor(position.X);
            int y = (int)Math.Floor(position.Y);

            float dx = position.X - x;
            float dy = position.Y - y;

            float[,] s = new float[2, 2];
            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    Vector2 gradient = RandomUnit2(x + i, y + j);
                    Vector2 distance = new Vector2(dx - i, dy - j);
                    s[i, j] = Vector2.Dot(gradient, distance);
                }
            }

            float sx = SCurve(dx);
            float sy = SCurve(dy);

            float y1 = LinearInterpolation(s[0, 0], s[1, 0], sx);
            float y2 = LinearInterpolation(s[0, 1], s[1, 1], sx);
            return LinearInterpolation(y1, y2, sy);
        }

        public static float Noise3(Vector3 position)
        {
            int x = (int)Math.Floor(position.X);
            int y = (int)Math.Floor(position.Y);
            int z = (int)Math.Floor(position.Z);

            float dx = position.X - x;
            float dy = position.Y - y;
            float dz = position.Z - z;

            float[,,] s = new float[2, 2, 2];
            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    for (int k = 0; k < 2; ++k)
                    {
                        Vector3 gradient = RandomUnit3(x + i, y + j, z + k);
                        Vector3 distance = new Vector3(dx - i, dy - j, dz - k);
                        s[i, j, k] = Vector3.Dot(gradient, distance);
                    }
                }
            }

            float sx = SCurve(dx);
            float sy = SCurve(dy);
            float sz = SCurve(dz);

            float z1y1 = LinearInterpolation(s[0, 0, 0], s[1, 0, 0], sx);
            float z1y2 = LinearInterpolation(s[0, 1, 0], s[1, 1, 0], sx);
            float z1 = LinearInterpolation(z1y1, z1y2, sy);

            float z2y1 = LinearInterpolation(s[0, 0, 1], s[1, 0, 1], sx);
            float z2y2 = LinearInterpolation(s[0, 1, 1], s[1, 1, 1], sx);
            float z2 = LinearInterpolation(z2y1, z2y2, sy);

            return LinearInterpolation(z1, z2, sz);
        }
    }
}
