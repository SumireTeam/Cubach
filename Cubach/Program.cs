using Cubach.Model;
using Cubach.View;
using Cubach.View.OpenGL;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OpenTK.Input;

namespace Cubach
{
    public class Program : IDisposable
    {
        private readonly Configuration config;
        private readonly IWindow window;

        private readonly IMeshFactory meshFactory = new GLMeshFactory();
        private readonly ITextureFactory<GLTexture> textureFactory = new GLTextureFactory();

        private SpriteBatch<GLTexture> spriteBatch;
        private TextRenderer<GLTexture> textRenderer;

        private TextureAtlas<GLTexture> blockTextureAtlas;
        private ShaderProgram blockProgram;
        private ShaderProgram textProgram;

        private readonly World world = new World();
        private readonly FirstPersonCamera camera;
        private WorldRenderer worldRenderer;

        private string vendor = "";
        private string renderer = "";
        private string version = "";
        private string glsl = "";

        private float avgFPS = 60;

        private Program()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            config = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("./config.json"));

            window = new GLWindow(width: 1024, height: 768, title: "Cubach");

            window.Load += Window_Load;
            window.Resize += Window_Resize;
            window.RenderFrame += Window_RenderFrame;

            var position = new Vector3(World.Length * Chunk.Length / 2f,
                World.Width * Chunk.Width / 2f,
                World.Height * Chunk.Height / 2f + 10);
            camera = new FirstPersonCamera(position);
        }

        private void LoadTextures()
        {
            Console.WriteLine("Stitching textures...");
            using (var builder = new TextureAtlasBuilder<GLTexture>(textureFactory)) {
                var files = Directory.GetFiles("./Textures/Blocks");
                foreach (var file in files) {
                    var name = Path.GetFileNameWithoutExtension(file);
                    builder.AddImage(name, new Bitmap(file));
                }

                blockTextureAtlas = builder.Build();
            }
        }

        private void LoadShaders()
        {
            Console.WriteLine("Compiling shaders...");

            using (var vs = Shader.Create(ShaderType.VertexShader, File.ReadAllText("./Shaders/cube.vert")))
            using (var fs = Shader.Create(ShaderType.FragmentShader, File.ReadAllText("./Shaders/cube.frag"))) {
                blockProgram = ShaderProgram.Create(vs, fs);

                var mvp = Matrix4.Identity;
                blockProgram.SetUniform("mvp", ref mvp);
                blockProgram.SetUniform("colorTexture", 0);
            }

            using (var vs = Shader.Create(ShaderType.VertexShader, File.ReadAllText("./Shaders/text.vert")))
            using (var fs = Shader.Create(ShaderType.FragmentShader, File.ReadAllText("./Shaders/text.frag"))) {
                textProgram = ShaderProgram.Create(vs, fs);

                var mvp = Matrix4.Identity;
                textProgram.SetUniform("mvp", ref mvp);
                textProgram.SetUniform("colorTexture", 0);
            }
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

            spriteBatch = new SpriteBatch<GLTexture>(meshFactory);
            textRenderer = new TextRenderer<GLTexture>(textureFactory, spriteBatch);

            LoadTextures();
            LoadShaders();

            var chunkRenderer = new ChunkRenderer(meshFactory, blockTextureAtlas);
            worldRenderer = new WorldRenderer(world, camera, chunkRenderer);
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

        private void Window_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, window.Width, window.Height);

            camera.Aspect = window.Width / (float) window.Height;
        }

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
            const float moveSpeed = 10f;

            var keyboardState = Keyboard.GetState();
            var moveDirection = Vector3.Zero;

            var front = camera.Front;
            var right = camera.Right;
            var up = camera.Up;

            if (keyboardState.IsKeyDown(Key.A)) {
                moveDirection -= right;
            }

            if (keyboardState.IsKeyDown(Key.D)) {
                moveDirection += right;
            }

            if (keyboardState.IsKeyDown(Key.W)) {
                moveDirection += front;
            }

            if (keyboardState.IsKeyDown(Key.S)) {
                moveDirection -= front;
            }

            if (keyboardState.IsKeyDown(Key.R)) {
                moveDirection += Vector3.UnitZ;
            }

            if (keyboardState.IsKeyDown(Key.F)) {
                moveDirection -= Vector3.UnitZ;
            }

            if (moveDirection.Length > 0) {
                camera.Position += moveDirection.Normalized() * moveSpeed * e.Time;
            }

            const float rotationSpeed = 1f;

            if (keyboardState.IsKeyDown(Key.Left)) {
                camera.Orientation =
                    Quaternion.FromAxisAngle(Vector3.UnitZ, rotationSpeed * e.Time) * camera.Orientation;
            }

            if (keyboardState.IsKeyDown(Key.Right)) {
                camera.Orientation =
                    Quaternion.FromAxisAngle(Vector3.UnitZ, -rotationSpeed * e.Time) * camera.Orientation;
            }

            if (keyboardState.IsKeyDown(Key.Up) && front.Z < 0.95f) {
                camera.Orientation *= Quaternion.FromAxisAngle(Vector3.UnitX, rotationSpeed * e.Time);
            }

            if (keyboardState.IsKeyDown(Key.Down) && front.Z > -0.95f) {
                camera.Orientation *= Quaternion.FromAxisAngle(Vector3.UnitX, -rotationSpeed * e.Time);
            }

            camera.Orientation.Normalize();

            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);

            blockTextureAtlas.Texture.Bind();

            float aspect = window.Width / (float) window.Height;
            worldRenderer.Draw(blockProgram, aspect, e.Time);

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);

            textProgram.Use();
            var mvp = Matrix4.CreateOrthographicOffCenter(0, window.Width, window.Height, 0, -1, 1);
            textProgram.SetUniform("mvp", ref mvp);

            // Apply exponential smoothing to the FPS.
            float fps = 1 / e.Time;
            avgFPS += 0.1f * (fps - avgFPS);

            var fpsStr = $"FPS {(int) Math.Floor(avgFPS)}";
            DrawString(new Vector2(8, 8), fpsStr);

            var position = camera.Position;
            var direction = camera.Front;
            var posStr = $"Pos {position.X:0.00}, {position.Y:0.00}, {position.Z:0.00}\n"
                         + $"Dir {direction.X:0.00}, {direction.Y:0.00}, {direction.Z:0.00}";
            DrawString(new Vector2(8, 8 + 1.5f * 16), posStr);

            var info = $"{vendor}\n{renderer}\n{version}\n{glsl}";
            var infoSize = textRenderer.MeasureString(config.FontFamily, config.FontSize, info);
            DrawString(new Vector2(window.Width - infoSize.X - 8, 8), info);
        }

        private void Run() => window.Run();

        public void Dispose()
        {
            textRenderer?.Dispose();
            spriteBatch?.Dispose();

            worldRenderer?.Dispose();

            textProgram?.Dispose();
            blockProgram?.Dispose();
            blockTextureAtlas?.Dispose();

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
