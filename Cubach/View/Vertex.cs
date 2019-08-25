using System.Runtime.InteropServices;
using OpenTK;

namespace Cubach.View
{
    public interface IVertex
    {
        VertexAttribute[] GetVertexAttributes();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexP3 : IVertex
    {
        public readonly Vector3 Position;

        public static readonly int SizeInBytes = Marshal.SizeOf<VertexP3>();
        public static readonly VertexAttribute[] VertexAttributes;

        static VertexP3()
        {
            VertexAttributes = new[] {
                new VertexAttribute(VertexAttributeType.Float, 3, SizeInBytes, 0),
            };
        }

        public VertexP3(Vector3 position)
        {
            Position = position;
        }

        public VertexAttribute[] GetVertexAttributes()
        {
            return VertexAttributes;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexP3N3 : IVertex
    {
        public readonly Vector3 Position;
        public readonly Vector3 Normal;

        public static readonly int SizeInBytes = Marshal.SizeOf<VertexP3N3>();
        public static readonly VertexAttribute[] VertexAttributes;

        static VertexP3N3()
        {
            VertexAttributes = new[] {
                new VertexAttribute(VertexAttributeType.Float, 3, SizeInBytes, 0),
                new VertexAttribute(VertexAttributeType.Float, 3, SizeInBytes, Vector3.SizeInBytes, normalized: true),
            };
        }

        public VertexP3N3(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }

        public VertexAttribute[] GetVertexAttributes()
        {
            return VertexAttributes;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexP3N3T2 : IVertex
    {
        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Vector2 TexCoord;

        public static readonly int SizeInBytes = Marshal.SizeOf<VertexP3N3T2>();
        public static readonly VertexAttribute[] VertexAttributes;

        static VertexP3N3T2()
        {
            VertexAttributes = new[] {
                new VertexAttribute(VertexAttributeType.Float, 3, SizeInBytes, 0),
                new VertexAttribute(VertexAttributeType.Float, 3, SizeInBytes, Vector3.SizeInBytes, normalized: true),
                new VertexAttribute(VertexAttributeType.Float, 2, SizeInBytes, 2 * Vector3.SizeInBytes),
            };
        }

        public VertexP3N3T2(Vector3 position, Vector3 normal, Vector2 texCoord)
        {
            Position = position;
            Normal = normal;
            TexCoord = texCoord;
        }

        public VertexAttribute[] GetVertexAttributes()
        {
            return VertexAttributes;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexP2T2C4 : IVertex
    {
        public readonly Vector2 Position;
        public readonly Vector2 TexCoord;
        public readonly Vector4 Color;

        public static readonly int SizeInBytes = Marshal.SizeOf<VertexP2T2C4>();
        public static readonly VertexAttribute[] VertexAttributes;

        static VertexP2T2C4()
        {
            VertexAttributes = new[] {
                new VertexAttribute(VertexAttributeType.Float, 2, SizeInBytes, 0),
                new VertexAttribute(VertexAttributeType.Float, 2, SizeInBytes, Vector2.SizeInBytes),
                new VertexAttribute(VertexAttributeType.Float, 4, SizeInBytes, 2 * Vector2.SizeInBytes),
            };
        }

        public VertexP2T2C4(Vector2 position, Vector2 texCoord, Vector4 color)
        {
            Position = position;
            TexCoord = texCoord;
            Color = color;
        }


        public VertexAttribute[] GetVertexAttributes()
        {
            return VertexAttributes;
        }
    }
}
