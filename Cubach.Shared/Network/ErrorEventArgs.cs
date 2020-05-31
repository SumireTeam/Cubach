using System;

namespace Cubach.Shared.Network
{
    public class ErrorEventArgs : EventArgs
    {
        public readonly string Error;

        public ErrorEventArgs(string error) => Error = error;
    }
}
