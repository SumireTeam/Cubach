using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Cubach.View.OpenGL
{
    public sealed class ShaderProgramHandle : GLObjectHandle
    {
        public ShaderProgramHandle(int handle) : base(handle) { }

        public static ShaderProgramHandle Create()
        {
            int handle = GL.CreateProgram();
            return new ShaderProgramHandle(handle);
        }

        protected override void ReleaseHandle()
        {
            GL.DeleteProgram(Handle);
        }
    }

    public sealed class ShaderProgram : IDisposable
    {
        public readonly ShaderProgramHandle Handle;

        public ShaderProgram()
        {
            Handle = ShaderProgramHandle.Create();
        }

        public void AttachShader(Shader shader)
        {
            GL.AttachShader((int)Handle, (int)shader.Handle);
        }

        public void Link()
        {
            GL.LinkProgram((int)Handle);

            GL.GetProgram((int)Handle, GetProgramParameterName.LinkStatus, out int status);
            if (status == 0)
            {
                string message = GL.GetProgramInfoLog((int)Handle);
                throw new Exception(message);
            }
        }

        public static ShaderProgram Create(params Shader[] shaders)
        {
            var shaderProgram = new ShaderProgram();
            foreach (var shader in shaders)
            {
                shaderProgram.AttachShader(shader);
            }

            shaderProgram.Link();
            return shaderProgram;
        }

        private readonly Dictionary<string, int> UniformLocationCache = new Dictionary<string, int>();

        public int GetUniformLocation(string name)
        {
            if (UniformLocationCache.TryGetValue(name, out int location))
            {
                return location;
            }

            return UniformLocationCache[name] = GL.GetUniformLocation((int)Handle, name);
        }

        public void SetUniform(string name, int value)
        {
            Use();
            GL.Uniform1(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, float value)
        {
            Use();
            GL.Uniform1(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, Vector2 value)
        {
            Use();
            GL.Uniform2(GetUniformLocation(name), ref value);
        }

        public void SetUniform(string name, Vector3 value)
        {
            Use();
            GL.Uniform3(GetUniformLocation(name), ref value);
        }

        public void SetUniform(string name, Vector4 value)
        {
            Use();
            GL.Uniform4(GetUniformLocation(name), ref value);
        }

        public void SetUniform(string name, ref Matrix3 value, bool transpose = false)
        {
            Use();
            GL.UniformMatrix3(GetUniformLocation(name), transpose, ref value);
        }

        public void SetUniform(string name, ref Matrix4 value, bool transpose = false)
        {
            Use();
            GL.UniformMatrix4(GetUniformLocation(name), transpose, ref value);
        }

        public void Use()
        {
            GL.UseProgram((int)Handle);
        }

        public void Dispose()
        {
            Handle.Dispose();
        }
    }
}
