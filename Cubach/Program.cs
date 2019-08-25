using Cubach.Model;
using Cubach.View;
using Cubach.View.OpenGL;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;

namespace Cubach
{
    public class Program : IDisposable
    {
        private readonly Configuration config;
        private readonly IWindow window;

        private IMeshFactory meshFactory;
        private ITextureFactory<GLTexture> textureFactory;
        private SpriteBatch<GLTexture> spriteBatch;
        private TextRenderer<GLTexture> textRenderer;
        private ChunkGenerator chunkGenerator;
        private Chunk chunk;
        private ChunkRenderer chunkRenderer;

        private IMesh<VertexP3N3T2> chunkMesh;

        private ShaderProgram blockProgram;
        private ShaderProgram textProgram;

        private string vendor = "";
        private string renderer = "";
        private string version = "";
        private string glsl = "";

        private float avgFPS = 60;
        private float h = 0;
        private float v = 0;

        private Program()
        {
            config = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("./config.json"));

            window = new GLWindow(width: 800, height: 600, title: "Cubach");

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
            Console.WriteLine(version);
            Console.WriteLine(glsl);

            meshFactory = new GLMeshFactory();
            textureFactory = new GLTextureFactory();
            spriteBatch = new SpriteBatch<GLTexture>(meshFactory);
            textRenderer = new TextRenderer<GLTexture>(textureFactory, spriteBatch);

            using (var vs = Shader.Create(ShaderType.VertexShader, File.ReadAllText("./Shaders/cube.vert")))
            using (var fs = Shader.Create(ShaderType.FragmentShader, File.ReadAllText("./Shaders/cube.frag")))
            {
                blockProgram = ShaderProgram.Create(vs, fs);

                Matrix4 mvp = Matrix4.Identity;
                blockProgram.SetUniform("mvp", ref mvp);
            }

            using (var vs = Shader.Create(ShaderType.VertexShader, File.ReadAllText("./Shaders/text.vert")))
            using (var fs = Shader.Create(ShaderType.FragmentShader, File.ReadAllText("./Shaders/text.frag")))
            {
                textProgram = ShaderProgram.Create(vs, fs);

                Matrix4 mvp = Matrix4.Identity;
                textProgram.SetUniform("mvp", ref mvp);

                textProgram.SetUniform("colorTexture", 0);
            }

            chunkGenerator = new ChunkGenerator();
            chunk = chunkGenerator.Create(0, 0, 0);
            chunkRenderer = new ChunkRenderer(meshFactory);
            chunkMesh = chunkRenderer.CreateChunkMesh(chunk);
        }

        private void Window_Resize(object sender, EventArgs e) => GL.Viewport(0, 0, window.Width, window.Height);

        private void Window_RenderFrame(object sender, TimeEventArgs e)
        {
            blockProgram.Use();

            float aspect = window.Width / (float)window.Height;
            const float fovx = MathHelper.PiOver2;
            const float fovEps = 0.1f;
            float fovy = Math.Min(Math.Max(fovx / aspect, fovEps), MathHelper.Pi - fovEps);

            h += e.Time * MathHelper.DegreesToRadians(30);
            v += e.Time;

            Vector3 position = new Vector3(15.5f, 15.5f, (float)(15.5 + 20 * Math.Sin(v))) + Matrix3.CreateRotationZ(h) * new Vector3(40.0f, 0.0f, 0.0f);
            Vector3 target = new Vector3(15.5f, 15.5f, 15.5f);

            Matrix4 model = Matrix4.Identity;
            Matrix4 view = Matrix4.LookAt(position, target, Vector3.UnitZ);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(fovy, aspect, 0.1f, 100);

            Matrix4 mvp = model * view * projection;
            blockProgram.SetUniform("mvp", ref mvp);
            chunkMesh.Draw();

            textProgram.Use();

            mvp = Matrix4.CreateOrthographicOffCenter(0, window.Width, window.Height, 0, -1, 1);
            textProgram.SetUniform("mvp", ref mvp);

            // Apply exponential smoothing to the FPS.
            float fps = 1 / e.Time;
            avgFPS += 0.1f * (fps - avgFPS);

            string text = $"FPS {(int)Math.Floor(avgFPS)}\n{vendor}\n{renderer}\n{version}\n{glsl}";
            textRenderer.DrawString(new Vector2(9, 9), config.FontFamily, config.FontSize, text, new Color4(0.1f, 0.1f, 0.1f, 1f));
            textRenderer.DrawString(new Vector2(8, 8), config.FontFamily, config.FontSize, text, new Color4(1f, 1f, 1f, 1f));
        }

        private void Run() => window.Run();

        public void Dispose()
        {
            textRenderer?.Dispose();
            spriteBatch?.Dispose();

            chunkMesh?.Dispose();

            textProgram?.Dispose();
            blockProgram?.Dispose();

            window.Dispose();
        }

        private static void Main()
        {
            using (var program = new Program())
            {
                program.Run();
            }
        }
    }
}
