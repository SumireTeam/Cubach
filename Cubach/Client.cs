using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Cubach.Model;
using Cubach.Network;
using Cubach.Network.Local;
using Cubach.Network.Remote;
using Cubach.View;
using Cubach.View.OpenGL;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace Cubach
{
    public class Client : IDisposable
    {
        private readonly Configuration config;
        private readonly World world = new World();
        private readonly IWindow window;
        private IServerConnection serverConnection;

        private readonly IMeshFactory meshFactory = new GLMeshFactory();
        private readonly ITextureFactory<GLTexture> textureFactory = new GLTextureFactory();

        private SpriteBatch<GLTexture> spriteBatch;
        private TextRenderer<GLTexture> textRenderer;

        private TextureAtlas<GLTexture> blockTextureAtlas;
        private ShaderProgram worldProgram;
        private ShaderProgram uiProgram;

        private readonly FirstPersonCamera camera;
        private WorldRenderer worldRenderer;

        private bool showInfo = false;

        private string vendor = "";
        private string renderer = "";
        private string version = "";
        private string glsl = "";

        private float avgFPS = 60;

        public static readonly ConcurrentQueue<Action> DeferredTasks = new ConcurrentQueue<Action>();

        public Client(Configuration config)
        {
            this.config = config;

            window = new GLWindow(config.Width, config.Height, "Cubach", config.Fullscreen);

            window.Load += Window_Load;
            window.Resize += Window_Resize;
            window.RenderFrame += Window_RenderFrame;

            camera = new FirstPersonCamera();
        }

        private void LoadTextures()
        {
            Console.WriteLine("[Client] Loading textures...");

            using (var builder = new TextureAtlasBuilder<GLTexture>(textureFactory)) {
                var files = Directory.GetFiles("./Textures/Blocks");
                foreach (var file in files) {
                    var name = Path.GetFileNameWithoutExtension(file);
                    builder.AddImage(name, new Bitmap(file));
                }

                blockTextureAtlas = builder.Build();
            }

            Console.WriteLine("[Client] Loaded textures");
        }

        private void LoadShaders()
        {
            Console.WriteLine("[Client] Loading shaders...");

            using (var vs = Shader.Create(ShaderType.VertexShader, File.ReadAllText("./Shaders/world.vert")))
            using (var fs = Shader.Create(ShaderType.FragmentShader, File.ReadAllText("./Shaders/world.frag"))) {
                worldProgram = ShaderProgram.Create(vs, fs);

                var mvp = Matrix4.Identity;
                worldProgram.SetUniform("mvp", ref mvp);
                worldProgram.SetUniform("colorTexture", 0);
            }

            using (var vs = Shader.Create(ShaderType.VertexShader, File.ReadAllText("./Shaders/ui.vert")))
            using (var fs = Shader.Create(ShaderType.FragmentShader, File.ReadAllText("./Shaders/ui.frag"))) {
                uiProgram = ShaderProgram.Create(vs, fs);

                var mvp = Matrix4.Identity;
                uiProgram.SetUniform("mvp", ref mvp);
                uiProgram.SetUniform("colorTexture", 0);
            }

            Console.WriteLine("[Client] Loaded shaders");
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
        }

        private void Window_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, window.Width, window.Height);

            camera.Aspect = window.Width / (float) window.Height;
        }

        private void DrawString(Vector2 position, string text)
        {
            var shadowColor = new Color4(0.1f, 0.1f, 0.1f, 1f);
            var textColor = new Color4(0.55f, 0.55f, 0.55f, 1f);
            var text2Color = new Color4(1f, 1f, 1f, 1f);

            var offset = new Vector2(-1, -1);

            textRenderer.DrawString(position - offset, config.FontFamily, config.FontSize, text, shadowColor);
            textRenderer.DrawString(position, config.FontFamily, config.FontSize, text, textColor);
            textRenderer.DrawString(position + offset, config.FontFamily, config.FontSize, text, text2Color);
        }

        private bool PreviousF3State = false;

        private void HandleInput(float elapsedTime)
        {
            if (!window.HasFocus) {
                return;
            }

            const float moveSpeed = 10f;
            const float rotationSpeed = 1f;

            var front = camera.Front;
            var right = camera.Right;
            var up = camera.Up;

            var keyboardState = Keyboard.GetState();
            var shift = keyboardState.IsKeyDown(Key.LShift);
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

            if (keyboardState.IsKeyDown(Key.Escape)) {
                window.Close();
            }

            var f3State = keyboardState.IsKeyDown(Key.F3);
            if (f3State && !PreviousF3State) {
                showInfo = !showInfo;
            }

            PreviousF3State = f3State;

            if (moveDirection.Length > 0) {
                world.Player.Position += moveDirection.Normalized() * moveSpeed * elapsedTime * (shift ? 5 : 1);
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
        }

        private void Window_RenderFrame(object sender, TimeEventArgs e)
        {
            serverConnection.ProcessMessages();

            HandleInput(e.Time);

            camera.Position = world.Player.Position;
            camera.Orientation = world.Player.Orientation;

            world.Update(e.Time);

            while (DeferredTasks.Count > 0) {
                if (DeferredTasks.TryDequeue(out var task)) {
                    task();
                }
            }

            GL.ClearColor(0.6f, 0.7f, 0.8f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);

            blockTextureAtlas.Texture.Bind();

            var aspect = window.Width / (float) window.Height;
            worldRenderer.Draw(worldProgram, aspect, e.Time);

            GL.Disable(EnableCap.DepthTest);

            uiProgram.Use();
            var mvp = Matrix4.CreateOrthographicOffCenter(0, window.Width, window.Height, 0, -1, 1);
            uiProgram.SetUniform("mvp", ref mvp);

            // Apply exponential smoothing to the FPS.
            var fps = 1 / e.Time;
            avgFPS += 0.1f * (fps - avgFPS);

            var fpsStr = $"FPS {(int) Math.Floor(avgFPS)}";
            DrawString(new Vector2(8, 8), fpsStr);

            if (showInfo) {
                var position = camera.Position;
                var direction = camera.Front;
                var fovy = MathHelper.RadiansToDegrees(camera.FieldOfViewY);
                var posStr = $"Pos {position.X:0.00}, {position.Y:0.00}, {position.Z:0.00}\n"
                             + $"Dir {direction.X:0.00}, {direction.Y:0.00}, {direction.Z:0.00}\n"
                             + $"FOV {fovy:0.00}";
                DrawString(new Vector2(8, 8 + 1.5f * 16), posStr);

                var info = $"{vendor}\n{renderer}\n{version}\n{glsl}";
                var infoSize = textRenderer.MeasureString(config.FontFamily, config.FontSize, info);
                DrawString(new Vector2(window.Width - infoSize.X - 8, 8), info);
            }
        }

        private void Run()
        {
            serverConnection.Error += (s, e) => {
                var error = e.Error;
                Console.WriteLine($"[Client] Error: {error}");
            };

            serverConnection.Connected += (s, e) => {
                Console.WriteLine($"[Client] Connected");
                serverConnection.RequestChunks();
            };

            serverConnection.Disconnected += (s, e) => { Console.WriteLine($"[Client] Disconnected"); };

            serverConnection.ChunkReceived += (s, e) => {
                var chunk = e.Chunk;
                world.SetChunk(chunk);
            };

            serverConnection.Run();
            window.Run();
        }

        public void ConnectLocal(Queue<ServerMessage> incomingMessages, Queue<ClientMessage> outgoingMessages)
        {
            serverConnection = new LocalServerConnection(incomingMessages, outgoingMessages);
            Run();
        }

        public void ConnectRemote(string host, int port)
        {
            serverConnection = new RemoteServerConnection(host, port);
            Run();
        }

        public void Dispose()
        {
            textRenderer?.Dispose();
            spriteBatch?.Dispose();

            worldRenderer?.Dispose();

            uiProgram?.Dispose();
            worldProgram?.Dispose();
            blockTextureAtlas?.Dispose();

            window.Dispose();
        }
    }
}
