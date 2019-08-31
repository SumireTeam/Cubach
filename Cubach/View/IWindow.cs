using System;

namespace Cubach.View
{
    public class TimeEventArgs : EventArgs
    {
        public readonly float Time;

        public TimeEventArgs(float time) : base() => Time = time;
    }

    public interface IWindow : IDisposable
    {
        event EventHandler Load;
        event EventHandler Unload;
        event EventHandler Resize;
        event EventHandler<TimeEventArgs> RenderFrame;

        int Width { get; }
        int Height { get; }

        void Run();
        void MakeCurrent();
    }
}
