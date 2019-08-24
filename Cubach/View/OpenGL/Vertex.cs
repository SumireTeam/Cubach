using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Cubach.View.OpenGL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexP3
    {
        public readonly Vector3 Position;

        public VertexP3(Vector3 position)
        {
            Position = position;
        }

        public static readonly VertexAttribute[] VertexAttributes = new[] {
            new VertexAttribute(VertexAttribPointerType.Float, 3, Marshal.SizeOf<VertexP3>(), 0),
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexP3N3
    {
        public readonly Vector3 Position;
        public readonly Vector3 Normal;

        public VertexP3N3(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }

        public static readonly VertexAttribute[] VertexAttributes = new[] {
            new VertexAttribute(VertexAttribPointerType.Float, 3, Marshal.SizeOf<VertexP3N3>(), 0),
            new VertexAttribute(VertexAttribPointerType.Float, 3, Marshal.SizeOf<VertexP3N3>(), Vector3.SizeInBytes, true),
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexP3N3T2
    {
        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Vector2 TexCoord;

        public VertexP3N3T2(Vector3 position, Vector3 normal, Vector2 texCoord)
        {
            Position = position;
            Normal = normal;
            TexCoord = texCoord;
        }

        public static readonly VertexAttribute[] VertexAttributes = new[] {
            new VertexAttribute(VertexAttribPointerType.Float, 3, Marshal.SizeOf<VertexP3N3T2>(), 0),
            new VertexAttribute(VertexAttribPointerType.Float, 3, Marshal.SizeOf<VertexP3N3T2>(), Vector3.SizeInBytes, true),
            new VertexAttribute(VertexAttribPointerType.Float, 2, Marshal.SizeOf<VertexP3N3T2>(), 2 * Vector3.SizeInBytes),
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexP2T2C4
    {
        public readonly Vector2 Position;
        public readonly Vector2 TexCoord;
        public readonly Vector4 Color;

        public VertexP2T2C4(Vector2 position, Vector2 texCoord, Vector4 color)
        {
            Position = position;
            TexCoord = texCoord;
            Color = color;
        }

        public static readonly VertexAttribute[] VertexAttributes = new[] {
            new VertexAttribute(VertexAttribPointerType.Float, 2, Marshal.SizeOf<VertexP2T2C4>(), 0),
            new VertexAttribute(VertexAttribPointerType.Float, 2, Marshal.SizeOf<VertexP2T2C4>(), Vector2.SizeInBytes),
            new VertexAttribute(VertexAttribPointerType.Float, 4, Marshal.SizeOf<VertexP2T2C4>(), 2 * Vector2.SizeInBytes),
        };
    }
}
