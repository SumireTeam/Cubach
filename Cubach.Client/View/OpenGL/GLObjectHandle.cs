using System;

namespace Cubach.View.OpenGL
{
    public abstract class GLObjectHandle : IDisposable
    {
        public int Handle { get; private set; }

        public GLObjectHandle(int handle) => Handle = handle;

        public static explicit operator int(GLObjectHandle handle) => handle.Handle;

        public static explicit operator IntPtr(GLObjectHandle handle) => (IntPtr)handle.Handle;

        protected abstract void ReleaseHandle();

        public void Dispose()
        {
            ReleaseHandle();
            GC.SuppressFinalize(this);
        }

        ~GLObjectHandle()
        {
            ReleaseHandle();
        }
    }
}
