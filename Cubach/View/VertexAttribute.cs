namespace Cubach.View
{
    public enum VertexAttributeType
    {
        Float,
    }

    public struct VertexAttribute
    {
        public readonly VertexAttributeType Type;
        public readonly int Size;
        public readonly bool Normalized;
        public readonly int Stride;
        public readonly int Offset;

        public VertexAttribute(VertexAttributeType type, int size, int stride, int offset, bool normalized = false)
        {
            Type = type;
            Size = size;
            Normalized = normalized;
            Stride = stride;
            Offset = offset;
        }
    }
}
