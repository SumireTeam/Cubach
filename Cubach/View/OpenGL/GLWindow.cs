using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;

namespace Cubach.View.OpenGL
{
    public class GLWindow : IWindow
    {
        private readonly GameWindow window;

        private VertexArray VAO;
        private VertexBuffer VBO;
        private ShaderProgram Program;

        public GLWindow(int width, int height, string title)
        {
            var graphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), depth: 24, stencil: 0, samples: 4, accum: ColorFormat.Empty, buffers: 3);
            window = new GameWindow(width, height, graphicsMode, title, GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.ForwardCompatible);
            window.Load += Window_Load;
            window.Resize += Window_Resize;
            window.RenderFrame += Window_RenderFrame;
        }

        private void Window_Load(object sender, EventArgs e)
        {
            Console.WriteLine("Renderer: {0} {1} {2}", GL.GetString(StringName.Vendor), GL.GetString(StringName.Renderer), GL.GetString(StringName.Version));
            Console.WriteLine("GLSL version: {0}", GL.GetString(StringName.ShadingLanguageVersion));

            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.DepthTest);

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);

            //GL.Enable(EnableCap.Texture2D);

            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.CullFace(CullFaceMode.Back);

            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            var vertexData = new[] {
                // -X
                new VertexP3N3T2(new Vector3(0, 0, 0), -Vector3.UnitX, new Vector2(0, 0)),
                new VertexP3N3T2(new Vector3(0, 0, 1), -Vector3.UnitX, new Vector2(0, 1)),
                new VertexP3N3T2(new Vector3(0, 1, 1), -Vector3.UnitX, new Vector2(1, 1)),

                new VertexP3N3T2(new Vector3(0, 0, 0), -Vector3.UnitX, new Vector2(0, 0)),
                new VertexP3N3T2(new Vector3(0, 1, 1), -Vector3.UnitX, new Vector2(1, 1)),
                new VertexP3N3T2(new Vector3(0, 1, 0), -Vector3.UnitX, new Vector2(1, 0)),
                
                // +X
                new VertexP3N3T2(new Vector3(1, 0, 0), Vector3.UnitX, new Vector2(0, 0)),
                new VertexP3N3T2(new Vector3(1, 1, 1), Vector3.UnitX, new Vector2(1, 1)),
                new VertexP3N3T2(new Vector3(1, 0, 1), Vector3.UnitX, new Vector2(0, 1)),

                new VertexP3N3T2(new Vector3(1, 0, 0), Vector3.UnitX, new Vector2(0, 0)),
                new VertexP3N3T2(new Vector3(1, 1, 0), Vector3.UnitX, new Vector2(1, 0)),
                new VertexP3N3T2(new Vector3(1, 1, 1), Vector3.UnitX, new Vector2(1, 1)),
                
                // -Y
                new VertexP3N3T2(new Vector3(0, 0, 0), -Vector3.UnitY, new Vector2(0, 0)),
                new VertexP3N3T2(new Vector3(1, 0, 1), -Vector3.UnitY, new Vector2(1, 1)),
                new VertexP3N3T2(new Vector3(0, 0, 1), -Vector3.UnitY, new Vector2(0, 1)),

                new VertexP3N3T2(new Vector3(0, 0, 0), -Vector3.UnitY, new Vector2(0, 0)),
                new VertexP3N3T2(new Vector3(1, 0, 0), -Vector3.UnitY, new Vector2(1, 0)),
                new VertexP3N3T2(new Vector3(1, 0, 1), -Vector3.UnitY, new Vector2(1, 1)),
                
                // +Y
                new VertexP3N3T2(new Vector3(0, 1, 0), Vector3.UnitY, new Vector2(0, 0)),
                new VertexP3N3T2(new Vector3(0, 1, 1), Vector3.UnitY, new Vector2(0, 1)),
                new VertexP3N3T2(new Vector3(1, 1, 1), Vector3.UnitY, new Vector2(1, 1)),

                new VertexP3N3T2(new Vector3(0, 1, 0), Vector3.UnitY, new Vector2(0, 0)),
                new VertexP3N3T2(new Vector3(1, 1, 1), Vector3.UnitY, new Vector2(1, 1)),
                new VertexP3N3T2(new Vector3(1, 1, 0), Vector3.UnitY, new Vector2(1, 0)),
                
                // -Z
                new VertexP3N3T2(new Vector3(0, 0, 0), -Vector3.UnitZ, new Vector2(0, 0)),
                new VertexP3N3T2(new Vector3(0, 1, 0), -Vector3.UnitZ, new Vector2(0, 1)),
                new VertexP3N3T2(new Vector3(1, 1, 0), -Vector3.UnitZ, new Vector2(1, 1)),

                new VertexP3N3T2(new Vector3(0, 0, 0), -Vector3.UnitZ, new Vector2(0, 0)),
                new VertexP3N3T2(new Vector3(1, 1, 0), -Vector3.UnitZ, new Vector2(1, 1)),
                new VertexP3N3T2(new Vector3(1, 0, 0), -Vector3.UnitZ, new Vector2(1, 0)),
                
                // +Z
                new VertexP3N3T2(new Vector3(0, 0, 1), Vector3.UnitZ, new Vector2(0, 0)),
                new VertexP3N3T2(new Vector3(1, 1, 1), Vector3.UnitZ, new Vector2(1, 1)),
                new VertexP3N3T2(new Vector3(0, 1, 1), Vector3.UnitZ, new Vector2(0, 1)),

                new VertexP3N3T2(new Vector3(0, 0, 1), Vector3.UnitZ, new Vector2(0, 0)),
                new VertexP3N3T2(new Vector3(1, 0, 1), Vector3.UnitZ, new Vector2(1, 0)),
                new VertexP3N3T2(new Vector3(1, 1, 1), Vector3.UnitZ, new Vector2(1, 1)),
            };

            VAO = new VertexArray();

            VBO = new VertexBuffer();
            VBO.SetData(vertexData);

            for (int i = 0; i < VertexP3N3T2.VertexAttributes.Length; ++i)
            {
                VertexAttribute attribute = VertexP3N3T2.VertexAttributes[i];
                VAO.SetVertexAttribute(i, attribute, VBO);
            }

            VertexArray.Unbind();

            string vsSrc = @"#version 400

uniform mat4 mvp;

layout(location = 0) in vec3 vert_position;
layout(location = 1) in vec3 vert_normal;
layout(location = 2) in vec2 vert_texCoord;

out vec3 frag_position;
out vec3 frag_normal;
out vec2 frag_texCoord;

void main()
{
    frag_position = vert_position;
    frag_normal = vert_normal;
    frag_texCoord = vert_texCoord;
    gl_Position = mvp * vec4(vert_position, 1.0);
}";

            string fsSrc = @"#version 400

in vec3 frag_position;
in vec3 frag_normal;
in vec2 frag_texCoord;

layout(location = 0) out vec4 out_color;

void main()
{
    vec3 light = normalize(vec3(0.2, 0.4, 0.8));
    vec3 ambient = vec3(0.2);
    vec3 diffuse = mix(vec3(0.0, 0.0, 0.1), vec3(0.5, 0.5, 0.4), (dot(frag_normal, light) + 1) / 2);
    out_color = vec4(ambient + diffuse, 1.0);
}";

            using (var vs = new Shader(ShaderType.VertexShader))
            using (var fs = new Shader(ShaderType.FragmentShader))
            {
                vs.Compile(vsSrc);
                fs.Compile(fsSrc);

                Program = new ShaderProgram();
                Program.AttachShader(vs);
                Program.AttachShader(fs);
                Program.Link();
            }

            Resize();
        }

        private void Window_Resize(object sender, EventArgs e)
        {
            Resize();
        }

        private void Resize()
        {
            GL.Viewport(0, 0, window.Width, window.Height);
        }

        private float h = 0;
        private float v = 0;

        private void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.ClearColor(0, 0, 0, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Program.Use();

            float aspect = window.Width / (float)window.Height;
            const float fovx = MathHelper.PiOver2;
            const float fovEps = 0.1f;
            float fovy = Math.Min(Math.Max(fovx / aspect, fovEps), MathHelper.Pi - fovEps);

            h += (float)(e.Time * MathHelper.DegreesToRadians(30));
            v += (float)e.Time;

            Vector3 position = new Vector3(0.5f, 0.5f, (float)(0.5 + Math.Sin(v))) + Matrix3.CreateRotationZ(h) * new Vector3(2.0f, 0.0f, 0.0f);
            Vector3 target = new Vector3(0.5f, 0.5f, 0.5f);

            Matrix4 model = Matrix4.Identity;
            Matrix4 view = Matrix4.LookAt(position, target, Vector3.UnitZ);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(fovy, aspect, 0.1f, 100);

            Matrix4 mvp = model * view * projection;

            Program.SetUniform("mvp", ref mvp);

            VAO.Draw(PrimitiveType.Triangles, 0, 36);

            window.SwapBuffers();
        }

        public void Run()
        {
            window.Run();
        }

        public void Dispose()
        {
            VAO.Dispose();
            VBO.Dispose();
            Program.Dispose();

            window.Dispose();
        }
    }
}
