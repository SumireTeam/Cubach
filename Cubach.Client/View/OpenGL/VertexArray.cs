using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace Cubach.View.OpenGL
{
    public sealed class VertexArrayHandle : GLObjectHandle
    {
        public VertexArrayHandle(int handle) : base(handle) { }

        public static VertexArrayHandle Create()
        {
            int handle = GL.GenVertexArray();
            return new VertexArrayHandle(handle);
        }

        protected override void ReleaseHandle() => GL.DeleteVertexArray(Handle);
    }

    public sealed class VertexArray : IDisposable
    {
        public readonly VertexArrayHandle Handle;

        public VertexArray() => Handle = VertexArrayHandle.Create();

        public void Bind() => GL.BindVertexArray((int)Handle);

        public static void Unbind() => GL.BindVertexArray(0);

        private static readonly Dictionary<VertexAttributeType, VertexAttribPointerType> typeMap = new Dictionary<VertexAttributeType, VertexAttribPointerType>
        {
            [VertexAttributeType.Float] = VertexAttribPointerType.Float,
            [VertexAttributeType.UnsignedByte] = VertexAttribPointerType.UnsignedByte,
        };

        public void SetVertexAttribute(int index, VertexAttribute attribute, VertexBuffer vertexBuffer)
        {
            Bind();
            vertexBuffer.Bind();
            GL.EnableVertexAttribArray(index);

            VertexAttribPointerType glVertexAttribType = typeMap[attribute.Type];
            GL.VertexAttribPointer(index, attribute.Size, glVertexAttribType, attribute.Normalized, attribute.Stride, attribute.Offset);

            VertexBuffer.Unbind();
        }

        public void Draw(PrimitiveType type, int first, int count)
        {
            Bind();
            GL.DrawArrays(type, first, count);
        }

        public void Dispose() => Handle.Dispose();
    }
}
