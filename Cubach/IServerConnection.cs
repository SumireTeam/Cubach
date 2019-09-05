using System;
using Cubach.Model;

namespace Cubach
{
    public interface IServerConnection
    {
        event EventHandler<ChunkEventArgs> ChunkReceived;

        void ProcessMessages();
        void RequestChunks();
    }
}
