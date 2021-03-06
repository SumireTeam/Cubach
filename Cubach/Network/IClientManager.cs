using System;
using Cubach.Model;

namespace Cubach.Network
{
    public interface IClientManager
    {
        event EventHandler<ErrorEventArgs> Error;
        event EventHandler<ClientConnectionEventArgs> Connected;
        event EventHandler<ClientConnectionEventArgs> Disconnected;
        event EventHandler<ClientConnectionEventArgs> ChunksRequestReceived;

        void Run();
        void ProcessMessages();
        void SendChunk(Chunk chunk);
    }
}
