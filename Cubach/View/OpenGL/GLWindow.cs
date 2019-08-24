using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;
using System.Threading;

namespace Cubach.View.OpenGL
{
    public class Configuration
    {
        public readonly string FontFamily;
        public readonly int FontSize;

        public Configuration(string fontFamily, int fontSize)
        {
            FontFamily = fontFamily;
            FontSize = fontSize;
        }
    }

    public class GLWindow : IWindow
    {
        private readonly Configuration config;
        private readonly GameWindow window;

        private VertexArray VAO;
        private VertexBuffer VBO;
        private ShaderProgram CubeProgram;
        private ShaderProgram TextProgram;
        private SpriteBatch SpriteBatch;
        private GLTextRenderer TextRenderer;

        private float avgFPS = 60;
        private string vendor = "";
        private string renderer = "";
        private string version = "";
        private string glsl = "";

        public GLWindow(int width, int height, string title)
        {
            config = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("./config.json"));

            var graphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), depth: 24, stencil: 0, samples: 4, accum: ColorFormat.Empty, buffers: 3);
            window = new GameWindow(width, height, graphicsMode, title, GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.ForwardCompatible);
            window.Load += Window_Load;
            window.Resize += Window_Resize;
            window.RenderFrame += Window_RenderFrame;
        }

        private void Window_Load(object sender, EventArgs e)
        {
            vendor = string.Format("Vendor: {0}", GL.GetString(StringName.Vendor));
            renderer = string.Format("Renderer: {0}", GL.GetString(StringName.Renderer));
            version = string.Format("Version: {0}", GL.GetString(StringName.Version));
            glsl = string.Format("GLSL version: {0}", GL.GetString(StringName.ShadingLanguageVersion));

            Console.WriteLine(vendor);
            Console.WriteLine(renderer);
            Console.WriteLine(glsl);

            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.DepthTest);

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);

            GL.Enable(EnableCap.Texture2D);

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

            using (var vs = Shader.Create(ShaderType.VertexShader, File.ReadAllText("./Shaders/cube.vert")))
            using (var fs = Shader.Create(ShaderType.FragmentShader, File.ReadAllText("./Shaders/cube.frag")))
            {
                CubeProgram = ShaderProgram.Create(vs, fs);

                Matrix4 mvp = Matrix4.Identity;
                CubeProgram.SetUniform("mvp", ref mvp);
            }

            using (var vs = Shader.Create(ShaderType.VertexShader, File.ReadAllText("./Shaders/text.vert")))
            using (var fs = Shader.Create(ShaderType.FragmentShader, File.ReadAllText("./Shaders/text.frag")))
            {
                TextProgram = ShaderProgram.Create(vs, fs);

                Matrix4 mvp = Matrix4.Identity;
                TextProgram.SetUniform("mvp", ref mvp);

                TextProgram.SetUniform("colorTexture", 0);
            }

            SpriteBatch = new SpriteBatch();
            TextRenderer = new GLTextRenderer(SpriteBatch);

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

            CubeProgram.Use();

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
            CubeProgram.SetUniform("mvp", ref mvp);

            VAO.Draw(PrimitiveType.Triangles, 0, 36);

            TextProgram.Use();

            mvp = Matrix4.CreateOrthographicOffCenter(0, window.Width, window.Height, 0, -1, 1);
            TextProgram.SetUniform("mvp", ref mvp);

            // Apply exponential smoothing to the FPS.
            float fps = (float)(1 / e.Time);
            avgFPS += 0.1f * (fps - avgFPS);

            string text = $"FPS {(int)Math.Floor(avgFPS)}\n{vendor}\n{renderer}\n{version}\n{glsl}";
            TextRenderer.DrawString(new Vector2(8, 8), config.FontFamily, config.FontSize, text, new Color4(0.9f, 0.9f, 0.9f, 0.9f));

            window.SwapBuffers();
            Thread.Sleep(1);
        }

        public void Run()
        {
            window.Run();
        }

        public void Dispose()
        {
            TextRenderer.Dispose();
            SpriteBatch.Dispose();

            VAO.Dispose();
            VBO.Dispose();

            TextProgram.Dispose();
            CubeProgram.Dispose();

            window.Dispose();
        }
    }
}
