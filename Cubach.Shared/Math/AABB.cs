using OpenTK;
using SMath = System.Math;

namespace Cubach.Shared.Math
{
    public struct AABB
    {
        public readonly Vector3 Min;
        public readonly Vector3 Max;

        public AABB(Vector3 a, Vector3 b)
        {
            Min = new Vector3(SMath.Min(a.X, b.X), SMath.Min(a.Y, b.Y), SMath.Min(a.Z, b.Z));
            Max = new Vector3(SMath.Max(a.X, b.X), SMath.Max(a.Y, b.Y), SMath.Max(a.Z, b.Z));
        }

        public static AABB CreateOffCenter(Vector3 center, Vector3 size)
        {
            var halfSize = size * 0.5f;
            return new AABB(center - halfSize, center + halfSize);
        }

        public Vector3 Size => Max - Min;

        public float Length => Size.X;
        public float Width => Size.Y;
        public float Height => Size.Z;

        public Vector3[] Points => new[] {
            Min,
            new Vector3(Max.X, Min.Y, Min.Z),
            new Vector3(Min.X, Max.Y, Min.Z),
            new Vector3(Max.X, Max.Y, Min.Z),
            new Vector3(Min.X, Min.Y, Max.Z),
            new Vector3(Max.X, Min.Y, Max.Z),
            new Vector3(Min.X, Max.Y, Max.Z),
            Max,
        };

        public AABB Move(Vector3 offset)
        {
            return new AABB(Min + offset, Max + offset);
        }

        public bool HasIntersection(AABB other)
        {
            return !(Max.X < other.Min.X || Max.Y < other.Min.Y || Max.Z < other.Min.Z
                     || Min.X > other.Max.X || Min.Y > other.Max.Y || Min.Z > other.Max.Z);
        }
    }
}
