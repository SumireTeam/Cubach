using System;
using Cubach.Model;

namespace Cubach
{
    public interface IClientConnection
    {
        event EventHandler<EventArgs> ChunksRequestReceived;

        void ProcessMessages();
        void SendChunk(Chunk chunk);
    }
}
