using System;

namespace Cubach.Shared.Network
{
    public class ClientConnectionEventArgs : EventArgs
    {
        public readonly IClientConnection Connection;

        public ClientConnectionEventArgs(IClientConnection connection) => Connection = connection;
    }
}
