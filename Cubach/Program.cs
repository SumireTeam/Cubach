using Cubach.Model;
using Cubach.View;
using Cubach.View.OpenGL;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;
using System.Threading.Tasks;

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
        private readonly World world = new World();
        private WorldRenderer worldRenderer;

        private ShaderProgram blockProgram;
        private ShaderProgram textProgram;

        private string vendor = "";
        private string renderer = "";
        private string version = "";
        private string glsl = "";

        private float avgFPS = 60;

        private bool isDisposed = false;

        private Program()
        {
            config = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("./config.json"));

            window = new GLWindow(width: 800, height: 600, title: "Cubach");

            window.Load += Window_Load;
            window.Resize += Window_Resize;
            window.RenderFrame += Window_RenderFrame;
        }

        private void LoadWorld(string path)
        {
            Console.WriteLine("Loading world...");

            using (var stream = File.OpenRead(path)) {
                world.Load(stream);
            }
        }

        private void SaveWorld(string path)
        {
            Console.WriteLine("Saving world...");

            using (var stream = File.OpenWrite(path)) {
                world.Save(stream);
            }
        }

        private void GenerateWorld()
        {
            Console.WriteLine("Generating world...");

            var randomProvider = new RandomProvider(0);
            var noiseProvider = new PerlinNoise(randomProvider);
            var chunkGenerator = new ChunkGenerator(noiseProvider);
            var worldGenerator = new WorldGenerator(world, chunkGenerator);
            worldGenerator.ChunkGenerated += (s, ev) => { worldRenderer.RequestChunkUpdate(ev.X, ev.Y, ev.Z); };
            worldGenerator.CreateChunks();
        }

        private void Window_Load(object sender, EventArgs e)
        {
            vendor = $"Vendor: {GL.GetString(StringName.Vendor)}";
            renderer = $"Renderer: {GL.GetString(StringName.Renderer)}";
            version = $"Version: {GL.GetString(StringName.Version)}";
            glsl = $"GLSL version: {GL.GetString(StringName.ShadingLanguageVersion)}";

            Console.WriteLine(vendor);
            Console.WriteLine(renderer);
            Console.WriteLine(version);
            Console.WriteLine(glsl);

            meshFactory = new GLMeshFactory();
            textureFactory = new GLTextureFactory();
            spriteBatch = new SpriteBatch<GLTexture>(meshFactory);
            textRenderer = new TextRenderer<GLTexture>(textureFactory, spriteBatch);

            using (var vs = Shader.Create(ShaderType.VertexShader, File.ReadAllText("./Shaders/cube.vert")))
            using (var fs = Shader.Create(ShaderType.FragmentShader, File.ReadAllText("./Shaders/cube.frag"))) {
                blockProgram = ShaderProgram.Create(vs, fs);

                var mvp = Matrix4.Identity;
                blockProgram.SetUniform("mvp", ref mvp);
            }

            using (var vs = Shader.Create(ShaderType.VertexShader, File.ReadAllText("./Shaders/text.vert")))
            using (var fs = Shader.Create(ShaderType.FragmentShader, File.ReadAllText("./Shaders/text.frag"))) {
                textProgram = ShaderProgram.Create(vs, fs);

                var mvp = Matrix4.Identity;
                textProgram.SetUniform("mvp", ref mvp);

                textProgram.SetUniform("colorTexture", 0);
            }

            var chunkRenderer = new ChunkRenderer(meshFactory);
            worldRenderer = new WorldRenderer(world, chunkRenderer);
            world.ChunkUpdated += (s, ev) => { worldRenderer.RequestChunkUpdate(ev.X, ev.Y, ev.Z); };

            const string worldFileName = "world.bin";
            var worldFilePath = Path.Combine(config.SavePath, worldFileName);
            if (File.Exists(worldFilePath)) {
                // Load world from the file.
                Task.Run(() => { LoadWorld(worldFilePath); });
            }
            else {
                // Generate world and save it to the file.
                Task.Run(() => {
                    GenerateWorld();
                    SaveWorld(worldFilePath);
                });
            }
        }

        private void Window_Resize(object sender, EventArgs e) => GL.Viewport(0, 0, window.Width, window.Height);

        private void DrawString(Vector2 position, string text)
        {
            var shadowPos = position + new Vector2(1, 1);
            var shadowColor = new Color4(0.1f, 0.1f, 0.1f, 1f);
            var textColor = new Color4(1f, 1f, 1f, 1f);
            textRenderer.DrawString(shadowPos, config.FontFamily, config.FontSize, text, shadowColor);
            textRenderer.DrawString(position, config.FontFamily, config.FontSize, text, textColor);
        }

        private void Window_RenderFrame(object sender, TimeEventArgs e)
        {
            float aspect = window.Width / (float) window.Height;
            worldRenderer.Draw(blockProgram, aspect, e.Time);

            textProgram.Use();
            var mvp = Matrix4.CreateOrthographicOffCenter(0, window.Width, window.Height, 0, -1, 1);
            textProgram.SetUniform("mvp", ref mvp);

            // Apply exponential smoothing to the FPS.
            float fps = 1 / e.Time;
            avgFPS += 0.1f * (fps - avgFPS);

            var fpsStr = $"FPS {(int) Math.Floor(avgFPS)}";
            DrawString(new Vector2(8, 8), fpsStr);

            var info = $"{vendor}\n{renderer}\n{version}\n{glsl}";
            var infoSize = textRenderer.MeasureString(config.FontFamily, config.FontSize, info);
            DrawString(new Vector2(window.Width - infoSize.X - 8, 8), info);
        }

        private void Run() => window.Run();

        public void Dispose()
        {
            isDisposed = true;

            textRenderer?.Dispose();
            spriteBatch?.Dispose();

            worldRenderer?.Dispose();

            textProgram?.Dispose();
            blockProgram?.Dispose();

            window.Dispose();
        }

        private static void Main()
        {
            using (var program = new Program()) {
                program.Run();
            }
        }
    }
}
