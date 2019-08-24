using OpenTK.Graphics.OpenGL4;

namespace Cubach.View
{
    public struct VertexAttribute
    {
        public readonly VertexAttribPointerType Type;
        public readonly int Size;
        public readonly bool Normalized;
        public readonly int Stride;
        public readonly int Offset;

        public VertexAttribute(VertexAttribPointerType type, int size, int stride, int offset, bool normalized = false)
        {
            Type = type;
            Size = size;
            Normalized = normalized;
            Stride = stride;
            Offset = offset;
        }
    }
}
