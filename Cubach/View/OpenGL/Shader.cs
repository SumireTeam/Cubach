using System;
using OpenTK.Graphics.OpenGL4;

namespace Cubach.View.OpenGL
{
    public sealed class ShaderHandle : GLObjectHandle
    {
        public ShaderHandle(int handle) : base(handle) { }

        public static ShaderHandle Create(ShaderType type)
        {
            int handle = GL.CreateShader(type);
            return new ShaderHandle(handle);
        }

        protected override void ReleaseHandle() => GL.DeleteShader(Handle);
    }

    public sealed class Shader : IDisposable
    {
        public readonly ShaderType Type;
        public readonly ShaderHandle Handle;

        public Shader(ShaderType type)
        {
            Type = type;
            Handle = ShaderHandle.Create(type);
        }

        public void Compile(string source)
        {
            GL.ShaderSource((int)Handle, source);
            GL.CompileShader((int)Handle);

            GL.GetShader((int)Handle, ShaderParameter.CompileStatus, out int status);
            if (status == 0)
            {
                string message = GL.GetShaderInfoLog((int)Handle);
                throw new Exception(message);
            }
        }

        public static Shader Create(ShaderType type, string source)
        {
            var shader = new Shader(type);
            shader.Compile(source);
            return shader;
        }

        public void Dispose() => Handle.Dispose();
    }
}
