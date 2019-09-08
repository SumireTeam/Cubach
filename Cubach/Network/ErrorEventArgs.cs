using System;

namespace Cubach.Network
{
    public class ErrorEventArgs : EventArgs
    {
        public readonly string Error;

        public ErrorEventArgs(string error) => Error = error;
    }
}
