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

        public GLWindow(int width, int height, string title)
        {
            var graphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), depth: 24, stencil: 0, samples: 4, accum: ColorFormat.Empty, buffers: 3);
            window = new GameWindow(width, height, graphicsMode, title, GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.ForwardCompatible);

            window.Load += Window_Load;
            window.Resize += Window_Resize;
            window.RenderFrame += Window_RenderFrame;
            window.Unload += Window_Unload;
        }

        public int Width => window.Width;
        public int Height => window.Height;

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

        private void Window_Resize(object sender, EventArgs e)
        {
            Resize(sender, e);
        }

        private void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.ClearColor(0, 0, 0, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            RenderFrame(sender, new TimeEventArgs((float)e.Time));

            window.SwapBuffers();
            Thread.Sleep(1);
        }

        private void Window_Unload(object sender, EventArgs e)
        {
            Unload(sender, e);
        }

        public void Run()
        {
            window.Run();
        }

        public void Dispose()
        {
            window.Dispose();
        }
    }
}
