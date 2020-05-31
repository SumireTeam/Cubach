using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Cubach.Client.Network;
using Cubach.Shared;
using Cubach.Shared.Network;
using Cubach.View;
using Cubach.View.OpenGL;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using Configuration = Cubach.Shared.Configuration;

namespace Cubach.Client
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

        private readonly FirstPersonCamera camera;
        private WorldRenderer worldRenderer;

        private IUserInterface userInterface;

        public static readonly ConcurrentQueue<Action> DeferredTasks = new ConcurrentQueue<Action>();

        public Client(Configuration config)
        {
            this.config = config;

            window = new GLWindow(config.Width, config.Height, "Cubach", config.Fullscreen);

            window.Load += Window_Load;
            window.Resize += Window_Resize;
            window.KeyPress += Window_KeyPress;
            window.RenderFrame += Window_RenderFrame;

            camera = new FirstPersonCamera();
        }

        private void LoadTextures()
        {
            Console.WriteLine("[Client] Loading textures...");

            using (var builder = new TextureAtlasBuilder<GLTexture>(textureFactory))
            {
                var files = Directory.GetFiles("./Textures/Blocks");
                foreach (var file in files)
                {
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
            using (var fs = Shader.Create(ShaderType.FragmentShader, File.ReadAllText("./Shaders/world.frag")))
            {
                worldProgram = ShaderProgram.Create(vs, fs);

                var mvp = Matrix4.Identity;
                worldProgram.SetUniform("mvp", ref mvp);
                worldProgram.SetUniform("colorTexture", 0);
            }

            Console.WriteLine("[Client] Loaded shaders");
        }

        private void Window_Load(object sender, EventArgs e)
        {
            spriteBatch = new SpriteBatch<GLTexture>(meshFactory);
            textRenderer = new TextRenderer<GLTexture>(textureFactory, spriteBatch);

            LoadTextures();
            LoadShaders();

            userInterface = new ImGuiUserInterface(camera, (uint)window.Width, (uint)window.Height);

            var chunkRenderer = new ChunkRenderer(meshFactory, blockTextureAtlas);
            worldRenderer = new WorldRenderer(config, world, camera, chunkRenderer);
            world.ChunkUpdated += (s, ev) => { worldRenderer.RequestChunkUpdate(ev.Chunk.X, ev.Chunk.Y, ev.Chunk.Z); };
        }

        private void Window_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, window.Width, window.Height);

            camera.Aspect = window.Width / (float)window.Height;

            userInterface.Resize((uint)window.Width, (uint)window.Height);
        }

        private void Window_KeyPress(object sender, View.KeyPressEventArgs e)
        {
            userInterface.PressChar(e.KeyChar);
        }

        private void HandleInput(float elapsedTime)
        {
            if (!window.HasFocus)
            {
                return;
            }

            userInterface.Update(window.NativeWindow, elapsedTime);
            userInterface.HandleMouseInput(window.NativeWindow, Mouse.GetCursorState());

            bool keyboardCaptured = userInterface.HandleKeyboardInput(window.NativeWindow, Keyboard.GetState());
            if (keyboardCaptured)
            {
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

            if (keyboardState.IsKeyDown(Key.A))
            {
                moveDirection -= right;
            }

            if (keyboardState.IsKeyDown(Key.D))
            {
                moveDirection += right;
            }

            if (keyboardState.IsKeyDown(Key.W))
            {
                moveDirection += front;
            }

            if (keyboardState.IsKeyDown(Key.S))
            {
                moveDirection -= front;
            }

            if (keyboardState.IsKeyDown(Key.R))
            {
                moveDirection += Vector3.UnitZ;
            }

            if (keyboardState.IsKeyDown(Key.F))
            {
                moveDirection -= Vector3.UnitZ;
            }

            if (keyboardState.IsKeyDown(Key.Escape))
            {
                window.Close();
            }

            if (moveDirection.Length > 0)
            {
                world.Player.Position += moveDirection.Normalized() * moveSpeed * elapsedTime * (shift ? 5 : 1);
            }

            if (keyboardState.IsKeyDown(Key.Left))
            {
                world.Player.Orientation = Quaternion.FromAxisAngle(Vector3.UnitZ, rotationSpeed * elapsedTime) * world.Player.Orientation;
            }

            if (keyboardState.IsKeyDown(Key.Right))
            {
                world.Player.Orientation = Quaternion.FromAxisAngle(Vector3.UnitZ, -rotationSpeed * elapsedTime) * world.Player.Orientation;
            }

            if (keyboardState.IsKeyDown(Key.Up))
            {
                var newOrientation = world.Player.Orientation * Quaternion.FromAxisAngle(Vector3.UnitX, rotationSpeed * elapsedTime);
                var newFront = newOrientation * -Vector3.UnitZ;
                // Prevents camera flipping when looking up.
                if (Math.Sign(newFront.X) == Math.Sign(front.X) && Math.Sign(newFront.Y) == Math.Sign(front.Y))
                {
                    world.Player.Orientation = newOrientation;
                }
            }

            if (keyboardState.IsKeyDown(Key.Down))
            {
                var newOrientation = world.Player.Orientation * Quaternion.FromAxisAngle(Vector3.UnitX, -rotationSpeed * elapsedTime);
                var newFront = newOrientation * -Vector3.UnitZ;
                // Prevents camera flipping when looking down.
                if (Math.Sign(newFront.X) == Math.Sign(front.X) && Math.Sign(newFront.Y) == Math.Sign(front.Y))
                {
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

            while (DeferredTasks.Count > 0)
            {
                if (DeferredTasks.TryDequeue(out var task))
                {
                    task();
                }
            }

            GL.ClearColor(0.6f, 0.7f, 0.8f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);
            GL.Disable(EnableCap.StencilTest);

            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
            GL.PolygonMode(MaterialFace.Back, PolygonMode.Line);

            blockTextureAtlas.Texture.Bind();

            var aspect = window.Width / (float)window.Height;
            worldRenderer.Draw(worldProgram, aspect, e.Time);

            userInterface.Draw(e.Time);

            GL.Disable(EnableCap.ScissorTest);
        }

        private void Run()
        {
            serverConnection.Error += (s, e) =>
            {
                var error = e.Error;
                Console.WriteLine($"[Client] Error: {error}");
            };

            serverConnection.Connected += (s, e) =>
            {
                Console.WriteLine($"[Client] Connected");
                serverConnection.RequestChunks();
            };

            serverConnection.Disconnected += (s, e) => { Console.WriteLine($"[Client] Disconnected"); };

            serverConnection.ChunkReceived += (s, e) =>
            {
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
            serverConnection = new ServerConnection(host, port);
            Run();
        }

        public void Dispose()
        {
            textRenderer?.Dispose();
            spriteBatch?.Dispose();

            (userInterface as IDisposable)?.Dispose();

            worldRenderer?.Dispose();

            worldProgram?.Dispose();
            blockTextureAtlas?.Dispose();

            window.Dispose();
        }
    }
}
