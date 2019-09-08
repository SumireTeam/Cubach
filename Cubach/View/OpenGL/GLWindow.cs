using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Threading;

namespace Cubach.View.OpenGL
{
    public class GLWindow : IWindow
    {
        private readonly GameWindow window;

        public event EventHandler Load = (s, e) => { };
        public event EventHandler Unload = (s, e) => { };
        public event EventHandler Resize = (s, e) => { };
        public event EventHandler<TimeEventArgs> RenderFrame = (s, e) => { };

        public GLWindow(int width, int height, string title, bool fullscreen)
        {
            var colorFormat = new ColorFormat(8, 8, 8, 8);
            var graphicsMode = new GraphicsMode(colorFormat, depth: 24, stencil: 0, samples: 0,
                accum: ColorFormat.Empty, buffers: 3);
            var windowFlags = fullscreen ? GameWindowFlags.Fullscreen : GameWindowFlags.Default;
            window = new GameWindow(width, height, graphicsMode, title, windowFlags, DisplayDevice.Default,
                4, 0, GraphicsContextFlags.ForwardCompatible) {VSync = VSyncMode.On};

            window.Load += Window_Load;
            window.Resize += Window_Resize;
            window.RenderFrame += Window_RenderFrame;
            window.Unload += Window_Unload;
        }

        public int Width => window.Width;
        public int Height => window.Height;
        public bool HasFocus => window.Focused && window.Visible;

        private void Window_Load(object sender, EventArgs e)
        {
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

            Load(sender, e);
            Resize(sender, e);
        }

        private void Window_Resize(object sender, EventArgs e) => Resize(sender, e);

        private void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            RenderFrame(sender, new TimeEventArgs((float) e.Time));

            window.SwapBuffers();
            Thread.Sleep(1);
        }

        private void Window_Unload(object sender, EventArgs e) => Unload(sender, e);

        public void Run() => window.Run(60, 60);
        public void MakeCurrent() => window.MakeCurrent();
        public void Close() => window.Close();
        public void Dispose() => window.Dispose();
    }
}
