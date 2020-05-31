using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace Cubach.View.OpenGL
{
    public sealed class VertexBufferHandle : GLObjectHandle
    {
        public VertexBufferHandle(int handle) : base(handle) { }

        public static VertexBufferHandle Create()
        {
            int handle = GL.GenBuffer();
            return new VertexBufferHandle(handle);
        }

        protected override void ReleaseHandle() => GL.DeleteBuffer(Handle);
    }

    public sealed class VertexBuffer : IDisposable
    {
        public readonly BufferTarget BufferTarget;
        public readonly VertexBufferHandle Handle;

        public VertexBuffer(BufferTarget bufferTarget = BufferTarget.ArrayBuffer)
        {
            BufferTarget = bufferTarget;
            Handle = VertexBufferHandle.Create();
        }

        public void Bind() => GL.BindBuffer(BufferTarget, (int)Handle);

        public static void Unbind(BufferTarget bufferTarget = BufferTarget.ArrayBuffer) => GL.BindBuffer(bufferTarget, 0);

        public void SetData(IntPtr data, int size, BufferUsageHint hint = BufferUsageHint.StaticDraw)
        {
            Bind();
            GL.BufferData(BufferTarget, size, data, hint);
        }

        public void SetData<T>(T[] data, BufferUsageHint hint = BufferUsageHint.StaticDraw) where T : struct
        {
            Bind();
            GL.BufferData(BufferTarget, Marshal.SizeOf<T>() * data.Length, data, hint);
        }

        public void Dispose() => Handle.Dispose();
    }
}
