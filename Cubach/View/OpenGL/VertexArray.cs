using System;
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

        protected override void ReleaseHandle()
        {
            GL.DeleteVertexArray(Handle);
        }
    }

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

    public sealed class VertexArray : IDisposable
    {
        public readonly VertexArrayHandle Handle;

        public VertexArray()
        {
            Handle = VertexArrayHandle.Create();
        }

        public void Bind()
        {
            GL.BindVertexArray((int)Handle);
        }

        public static void Unbind()
        {
            GL.BindVertexArray(0);
        }

        public void SetVertexAttribute(int index, VertexAttribute attribute, VertexBuffer vertexBuffer)
        {
            Bind();
            vertexBuffer.Bind();
            GL.EnableVertexAttribArray(index);
            GL.VertexAttribPointer(index, attribute.Size, attribute.Type, attribute.Normalized, attribute.Stride, attribute.Offset);
            VertexBuffer.Unbind();
        }

        public void Draw(PrimitiveType type, int first, int count)
        {
            Bind();
            GL.DrawArrays(type, first, count);
        }

        public void Dispose()
        {
            Handle.Dispose();
        }
    }
}
