using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

namespace Cubach.View.OpenGL
{
    public class GLMesh<TVertex> : IMesh<TVertex> where TVertex : struct, IVertex
    {
        private readonly VertexArray VAO;
        private readonly VertexBuffer VBO;

        private int vertexCount = 0;

        public GLMesh()
        {
            VAO = new VertexArray();
            VBO = new VertexBuffer();
        }

        private static readonly Dictionary<MeshUsageHint, BufferUsageHint> usageMap = new Dictionary<MeshUsageHint, BufferUsageHint>
        {
            [MeshUsageHint.Static] = BufferUsageHint.StaticDraw,
            [MeshUsageHint.Dynamic] = BufferUsageHint.DynamicDraw,
            [MeshUsageHint.Stream] = BufferUsageHint.StreamDraw,
        };

        public void SetData(TVertex[] data, MeshUsageHint usage = MeshUsageHint.Static)
        {
            vertexCount = data.Length;

            if (vertexCount == 0)
            {
                return;
            }

            VertexAttribute[] attributes = data[0].GetVertexAttributes();
            for (int i = 0; i < attributes.Length; ++i)
            {
                VAO.SetVertexAttribute(i, attributes[i], VBO);
            }

            BufferUsageHint glUsage = usageMap[usage];
            VBO.SetData(data, glUsage);
        }

        private static readonly Dictionary<DrawPrimitiveType, PrimitiveType> drawTypeMap = new Dictionary<DrawPrimitiveType, PrimitiveType>
        {
            [DrawPrimitiveType.Triangles] = PrimitiveType.Triangles,
        };

        public void Draw(DrawPrimitiveType type = DrawPrimitiveType.Triangles)
        {
            if (vertexCount == 0)
            {
                return;
            }

            PrimitiveType glType = drawTypeMap[type];
            VAO.Draw(glType, 0, vertexCount);
        }

        public void Dispose()
        {
            VBO.Dispose();
            VAO.Dispose();
        }
    }
}
