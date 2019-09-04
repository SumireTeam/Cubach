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
using System.IO.Compression;
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

            camera = new FirstPersonCamera();
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

            using (var stream = new MemoryStream()) {
                using (var fileStream = File.OpenRead(path))
                using (var gzStream = new GZipStream(fileStream, CompressionMode.Decompress)) {
                    gzStream.CopyTo(stream);
                }

                stream.Seek(0, SeekOrigin.Begin);

                var serializer = new WorldSerializer();
                serializer.Load(stream, world);
            }
        }

        private void SaveWorld(string path)
        {
            Console.WriteLine("Saving world...");

            using (var fileStream = File.OpenWrite(path))
            using (var gzStream = new GZipStream(fileStream, CompressionMode.Compress)) {
                var serializer = new WorldSerializer();
                serializer.Save(gzStream, world);
            }
        }

        private void GenerateWorld()
        {
            Console.WriteLine("Generating world...");

            var randomProvider = new RandomProvider(0);
            var noiseProvider = new PerlinNoise(randomProvider);
            var chunkGenerator = new ChunkGenerator(noiseProvider);
            var worldGenerator = new WorldGenerator(world, chunkGenerator);
            worldGenerator.ChunkGenerated += (s, ev) => {
                worldRenderer.RequestChunkUpdate(ev.Chunk.X, ev.Chunk.Y, ev.Chunk.Z);

                if (ev.Chunk.X > 0) {
                    worldRenderer.RequestChunkUpdate(ev.Chunk.X - 1, ev.Chunk.Y, ev.Chunk.Z);
                }

                if (ev.Chunk.X < World.Length - 1) {
                    worldRenderer.RequestChunkUpdate(ev.Chunk.X + 1, ev.Chunk.Y, ev.Chunk.Z);
                }

                if (ev.Chunk.Y > 0) {
                    worldRenderer.RequestChunkUpdate(ev.Chunk.X, ev.Chunk.Y - 1, ev.Chunk.Z);
                }

                if (ev.Chunk.Y < World.Width - 1) {
                    worldRenderer.RequestChunkUpdate(ev.Chunk.X, ev.Chunk.Y + 1, ev.Chunk.Z);
                }

                if (ev.Chunk.Z > 0) {
                    worldRenderer.RequestChunkUpdate(ev.Chunk.X, ev.Chunk.Y, ev.Chunk.Z - 1);
                }

                if (ev.Chunk.Z < World.Height - 1) {
                    worldRenderer.RequestChunkUpdate(ev.Chunk.X, ev.Chunk.Y, ev.Chunk.Z + 1);
                }
            };
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
            worldRenderer = new WorldRenderer(config, world, camera, chunkRenderer);
            world.ChunkUpdated += (s, ev) => { worldRenderer.RequestChunkUpdate(ev.Chunk.X, ev.Chunk.Y, ev.Chunk.Z); };

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

        private void HandleInput(float elapsedTime)
        {
            const float moveSpeed = 10f;
            const float rotationSpeed = 1f;

            var front = camera.Front;
            var right = camera.Right;
            var up = camera.Up;

            var keyboardState = Keyboard.GetState();
            var moveDirection = Vector3.Zero;

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
                world.Player.Position += moveDirection.Normalized() * moveSpeed * elapsedTime;
            }

            if (keyboardState.IsKeyDown(Key.Left)) {
                world.Player.Orientation =
                    Quaternion.FromAxisAngle(Vector3.UnitZ, rotationSpeed * elapsedTime) * world.Player.Orientation;
            }

            if (keyboardState.IsKeyDown(Key.Right)) {
                world.Player.Orientation =
                    Quaternion.FromAxisAngle(Vector3.UnitZ, -rotationSpeed * elapsedTime) *
                    world.Player.Orientation;
            }

            if (keyboardState.IsKeyDown(Key.Up)) {
                var newOrientation =
                    world.Player.Orientation * Quaternion.FromAxisAngle(Vector3.UnitX, rotationSpeed * elapsedTime);
                var newFront = newOrientation * -Vector3.UnitZ;
                // Prevents camera flipping when looking up.
                if (Math.Sign(newFront.X) == Math.Sign(front.X) && Math.Sign(newFront.Y) == Math.Sign(front.Y)) {
                    world.Player.Orientation = newOrientation;
                }
            }

            if (keyboardState.IsKeyDown(Key.Down)) {
                var newOrientation =
                    world.Player.Orientation *
                    Quaternion.FromAxisAngle(Vector3.UnitX, -rotationSpeed * elapsedTime);
                var newFront = newOrientation * -Vector3.UnitZ;
                // Prevents camera flipping when looking down.
                if (Math.Sign(newFront.X) == Math.Sign(front.X) && Math.Sign(newFront.Y) == Math.Sign(front.Y)) {
                    world.Player.Orientation = newOrientation;
                }
            }

            world.Player.Orientation.Normalize();

            camera.Position = world.Player.Position;
            camera.Orientation = world.Player.Orientation;
        }

        private void Window_RenderFrame(object sender, TimeEventArgs e)
        {
            HandleInput(e.Time);
            world.Update(e.Time);

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
