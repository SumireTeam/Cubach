using System;

namespace Cubach.View
{
    public enum MeshUsageHint
    {
        Static,
        Stream,
        Dynamic,
    }

    public enum DrawPrimitiveType
    {
        Triangles,
    }

    public interface IMesh<TVertex> : IDisposable where TVertex : struct, IVertex
    {
        int VertexCount { get; }

        void SetData(TVertex[] data, MeshUsageHint usage = MeshUsageHint.Static);
        void Draw(DrawPrimitiveType type = DrawPrimitiveType.Triangles);
    }
}
