using Cubach.Shared.Math;
using OpenTK;

namespace Cubach.Shared
{
    public class Player
    {
        public static readonly Vector3 Size = new Vector3(0.75f, 0.75f, 1.75f);

        public Vector3 Position { get; set; }
        public Vector3 Speed { get; set; }
        public Quaternion Orientation { get; set; }

        public Player(Vector3 position, Quaternion orientation)
        {
            Position = position;
            Orientation = orientation;
        }

        public Player(Vector3 position)
            : this(position, Quaternion.FromEulerAngles(MathHelper.PiOver2, 0, 0)) { }

        public Player() : this(Vector3.Zero) { }

        public AABB AABB => AABB.CreateOffCenter(Position, Size);
    }
}
