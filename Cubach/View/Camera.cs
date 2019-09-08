using System;
using OpenTK;

namespace Cubach.View
{
    public interface ICamera
    {
        Vector3 Position { get; }
        Vector3 Front { get; }
        Vector3 Right { get; }
        Vector3 Up { get; }

        Matrix4 Projection { get; }
        Matrix4 View { get; }
    }

    public class FirstPersonCamera : ICamera
    {
        public static readonly float MinFieldOfViewY = MathHelper.DegreesToRadians(30);
        public static readonly float MaxFieldOfViewY = MathHelper.DegreesToRadians(100);

        public Vector3 Position { get; set; }
        public Quaternion Orientation { get; set; }

        public float FieldOfViewX = MathHelper.DegreesToRadians(90);
        public float Aspect { get; set; } = 4f / 3;
        public float NearPlane = 0.1f;
        public float FarPlane = 1024f;

        public FirstPersonCamera(Vector3 position, Quaternion orientation)
        {
            Position = position;
            Orientation = orientation;
        }

        public FirstPersonCamera(Vector3 position)
            : this(position, Quaternion.FromEulerAngles(MathHelper.PiOver2, 0, 0)) { }

        public FirstPersonCamera() : this(Vector3.Zero) { }

        public float FieldOfViewY =>
            Math.Min(Math.Max(MinFieldOfViewY, FieldOfViewX / Aspect), MaxFieldOfViewY);

        public Vector3 Front => Orientation * -Vector3.UnitZ;
        public Vector3 Right => Vector3.Cross(Front, Vector3.UnitZ);
        public Vector3 Up => Vector3.Cross(Right, Front);

        public Matrix4 Projection =>
            Matrix4.CreatePerspectiveFieldOfView(FieldOfViewY, Aspect, NearPlane, FarPlane);

        public Matrix4 View =>
            Matrix4.CreateTranslation(-Position) * Matrix4.CreateFromQuaternion(Orientation.Inverted());
    }
}
