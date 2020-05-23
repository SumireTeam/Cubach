using OpenTK;
using System;

namespace Cubach.View
{
    public class KeyPressEventArgs : EventArgs
    {
        public readonly char KeyChar;

        public KeyPressEventArgs(char keyChar) : base() => KeyChar = keyChar;
    }

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
        event EventHandler<KeyPressEventArgs> KeyPress;
        event EventHandler<TimeEventArgs> RenderFrame;

        INativeWindow NativeWindow { get; }
        int Width { get; }
        int Height { get; }
        bool HasFocus { get; }

        void Run();
        void MakeCurrent();
        void Close();
    }
}
