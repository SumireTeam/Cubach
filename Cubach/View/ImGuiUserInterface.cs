using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Cubach.View.OpenGL;
using ImGuiNET;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Cubach.View
{
    public class ImGuiUserInterface : IDisposable, IUserInterface
    {
        private readonly ICamera camera;
        private uint width;
        private uint height;

        private VertexArray vertexArray;
        private VertexBuffer vertexBuffer;
        private VertexBuffer indexBuffer;
        private GLTexture texture;
        private ShaderProgram shaderProgram;

        private uint vertexBufferSize = 10000;
        private uint indexBufferSize = 2000;

        private Vector2 scaleFactor = Vector2.One;
        private readonly ImFontPtr defaultFont;
        private MouseState prevMouseState;
        private KeyboardState prevKeyboardState;
        private readonly List<char> pressedChars = new List<char>();

        private string vendor = "";
        private string renderer = "";
        private string version = "";
        private string glsl = "";

        private float avgFPS = 60;

        private bool showInfo = false;
        private bool prevF3State = false;

        public ImGuiUserInterface(ICamera camera, uint width, uint height)
        {
            this.camera = camera;
            this.width = width;
            this.height = height;

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.AddFontDefault();
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
            io.MouseDrawCursor = true;

            ImGui.StyleColorsClassic();
            ImGui.GetStyle().FrameBorderSize = 1;
            ImGui.GetStyle().FrameRounding = 2;

            defaultFont = io.Fonts.AddFontFromFileTTF("./Fonts/OpenSans/font.ttf", 16);
            io.Fonts.Build();

            io.KeyMap[(int)ImGuiKey.Tab] = (int)Key.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Key.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Key.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Key.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Key.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Key.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Key.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Key.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Key.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Key.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Key.BackSpace;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Key.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Key.Escape;
            io.KeyMap[(int)ImGuiKey.Space] = (int)Key.Space;
            io.KeyMap[(int)ImGuiKey.A] = (int)Key.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Key.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Key.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Key.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Key.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Key.Z;

            vendor = $"Vendor: {GL.GetString(StringName.Vendor)}";
            renderer = $"Renderer: {GL.GetString(StringName.Renderer)}";
            version = $"Version: {GL.GetString(StringName.Version)}";
            glsl = $"GLSL version: {GL.GetString(StringName.ShadingLanguageVersion)}";

            CreateShaderProgram();
            CreateVertexArray();
            CreateTexture();
        }

        private void CreateVertexArray()
        {
            vertexBuffer = new VertexBuffer();
            vertexBuffer.SetData(IntPtr.Zero, (int)vertexBufferSize, BufferUsageHint.DynamicDraw);

            indexBuffer = new VertexBuffer(BufferTarget.ElementArrayBuffer);
            indexBuffer.SetData(IntPtr.Zero, (int)indexBufferSize, BufferUsageHint.DynamicDraw);

            vertexArray = new VertexArray();

            vertexBuffer.Bind();
            indexBuffer.Bind();

            vertexArray.SetVertexAttribute(0, new VertexAttribute(VertexAttributeType.Float, 2, Unsafe.SizeOf<ImDrawVert>(), 0), vertexBuffer);
            vertexArray.SetVertexAttribute(1, new VertexAttribute(VertexAttributeType.Float, 2, Unsafe.SizeOf<ImDrawVert>(), 8), vertexBuffer);
            vertexArray.SetVertexAttribute(2, new VertexAttribute(VertexAttributeType.UnsignedByte, 4, Unsafe.SizeOf<ImDrawVert>(), 16, true), vertexBuffer);

            VertexArray.Unbind();
        }

        private void CreateTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

            texture = new GLTexture();
            texture.Bind();

            GL.TextureStorage2D((int)texture.Handle, 1, SizedInternalFormat.Rgba8, width, height);
            GL.TextureSubImage2D((int)texture.Handle, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            io.Fonts.SetTexID((IntPtr)texture.Handle);
            io.Fonts.ClearTexData();
        }

        private void CreateShaderProgram()
        {
            using (var vs = Shader.Create(ShaderType.VertexShader, File.ReadAllText("./Shaders/ui.vert")))
            using (var fs = Shader.Create(ShaderType.FragmentShader, File.ReadAllText("./Shaders/ui.frag")))
            {
                shaderProgram = ShaderProgram.Create(vs, fs);
            }
        }

        public void Resize(uint width, uint height)
        {
            this.width = width;
            this.height = height;
        }

        public void PressChar(char keyChar)
        {
            pressedChars.Add(keyChar);
        }

        public void Update(INativeWindow window, float elapsedTime)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new Vector2(width / scaleFactor.X, height / scaleFactor.Y);
            io.DisplayFramebufferScale = scaleFactor;
            io.DeltaTime = elapsedTime;
        }

        public bool HandleMouseInput(INativeWindow window, MouseState mouseState)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.MouseDown[0] = mouseState.LeftButton == ButtonState.Pressed;
            io.MouseDown[1] = mouseState.RightButton == ButtonState.Pressed;
            io.MouseDown[2] = mouseState.MiddleButton == ButtonState.Pressed;

            var screenPoint = new System.Drawing.Point(mouseState.X, mouseState.Y);
            var point = window.PointToClient(screenPoint);
            io.MousePos = new Vector2(point.X, point.Y);

            io.MouseWheel = mouseState.Scroll.Y - prevMouseState.Scroll.Y;
            io.MouseWheelH = mouseState.Scroll.X - prevMouseState.Scroll.X;

            prevMouseState = mouseState;

            return io.WantCaptureMouse;
        }

        public bool HandleKeyboardInput(INativeWindow window, KeyboardState keyboardState)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                io.KeysDown[(int)key] = keyboardState.IsKeyDown(key);
            }

            foreach (char c in pressedChars)
            {
                io.AddInputCharacter(c);
            }

            pressedChars.Clear();

            io.KeyCtrl = keyboardState.IsKeyDown(Key.ControlLeft) || keyboardState.IsKeyDown(Key.ControlRight);
            io.KeyAlt = keyboardState.IsKeyDown(Key.AltLeft) || keyboardState.IsKeyDown(Key.AltRight);
            io.KeyShift = keyboardState.IsKeyDown(Key.ShiftLeft) || keyboardState.IsKeyDown(Key.ShiftRight);
            io.KeySuper = keyboardState.IsKeyDown(Key.WinLeft) || keyboardState.IsKeyDown(Key.WinRight);

            prevKeyboardState = keyboardState;

            bool keyboardCaptured = io.WantCaptureKeyboard;
            if (!keyboardCaptured)
            {
                var f3State = keyboardState.IsKeyDown(Key.F3);
                if (f3State && !prevF3State)
                {
                    showInfo = !showInfo;
                }

                prevF3State = f3State;
            }

            return keyboardCaptured;
        }

        public void Draw(float elapsedTime)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            ImGui.NewFrame();
            ImGui.PushFont(defaultFont);

            ImGui.SetNextWindowPos(new Vector2(8, 8));

            ImGui.Begin("fps", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar);

            // Apply exponential smoothing to the FPS.
            float fps = 1.0f / elapsedTime;
            avgFPS += 0.1f * (fps - avgFPS);

            ImGui.Text($"FPS {Math.Round(avgFPS)}");

            if (showInfo)
            {
                var position = camera.Position;
                var direction = camera.Front;
                string posStr = $"Pos {position.X:0.00}, {position.Y:0.00}, {position.Z:0.00}\n"
                    + $"Dir {direction.X:0.00}, {direction.Y:0.00}, {direction.Z:0.00}\n";
                ImGui.Text(posStr);
            }

            ImGui.End();

            if (showInfo)
            {
                ImGui.SetNextWindowPos(new Vector2(width - 400 - 8, 8));
                ImGui.SetNextWindowSize(new Vector2(400, 0));

                ImGui.Begin("info", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoInputs
                    | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar);

                string info = $"{vendor}\n{renderer}\n{version}\n{glsl}";
                ImGui.Text(info);

                ImGui.End();
            }

            ImGui.PopFont();
            ImGui.Render();

            ImDrawDataPtr data = ImGui.GetDrawData();
            if (data.CmdListsCount == 0)
            {
                return;
            }

            vertexArray.Bind();

            uint requiredVertexBufferSize = (uint)(data.TotalVtxCount * Unsafe.SizeOf<ImDrawVert>());
            if (requiredVertexBufferSize > vertexBufferSize)
            {
                vertexBufferSize = Math.Max(2 * vertexBufferSize, requiredVertexBufferSize);
                vertexBuffer.SetData(IntPtr.Zero, (int)vertexBufferSize, BufferUsageHint.DynamicDraw);
            }

            uint requiredIndexBufferSize = (uint)(data.TotalIdxCount * sizeof(ushort));
            if (requiredIndexBufferSize > indexBufferSize)
            {
                indexBufferSize = Math.Max(2 * indexBufferSize, requiredIndexBufferSize);
                indexBuffer.SetData(IntPtr.Zero, (int)indexBufferSize, BufferUsageHint.DynamicDraw);
            }

            uint vertexBufferOffset = 0;
            uint indexBufferOffset = 0;

            for (int i = 0; i < data.CmdListsCount; ++i)
            {
                ImDrawListPtr list = data.CmdListsRange[i];

                uint vertexDataSize = (uint)(list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>());
                uint indexDataSize = (uint)(list.IdxBuffer.Size * sizeof(ushort));

                vertexBuffer.Bind();
                GL.BufferSubData(vertexBuffer.BufferTarget, (IntPtr)vertexBufferOffset, (int)vertexDataSize, list.VtxBuffer.Data);

                indexBuffer.Bind();
                GL.BufferSubData(indexBuffer.BufferTarget, (IntPtr)indexBufferOffset, (int)indexDataSize, list.IdxBuffer.Data);

                vertexBufferOffset += vertexDataSize;
                indexBufferOffset += indexDataSize;
            }

            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, io.DisplaySize.X, io.DisplaySize.Y, 0, -1, 1);

            shaderProgram.Use();
            shaderProgram.SetUniform("font_texture", 0);
            shaderProgram.SetUniform("projection", ref projection);

            data.ScaleClipRects(io.DisplayFramebufferScale);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);

            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.StencilTest);

            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            uint indexOffset = 0;
            uint vertexOffset = 0;
            for (int i = 0; i < data.CmdListsCount; ++i)
            {
                ImDrawListPtr list = data.CmdListsRange[i];
                for (int j = 0; j < list.CmdBuffer.Size; ++j)
                {
                    ImDrawCmdPtr cmd = list.CmdBuffer[j];
                    if (cmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        texture.Bind();

                        Vector4 clip = cmd.ClipRect;
                        GL.Scissor((int)clip.X, (int)height - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));

                        GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)cmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(indexOffset * sizeof(ushort)), (int)vertexOffset);
                    }

                    indexOffset += cmd.ElemCount;
                }

                vertexOffset += (uint)list.VtxBuffer.Size;
            }

            VertexArray.Unbind();
        }

        public void Dispose()
        {
            shaderProgram?.Dispose();
            texture?.Dispose();
            indexBuffer?.Dispose();
            vertexBuffer?.Dispose();
            vertexArray?.Dispose();
        }
    }
}
