using System;

namespace Cubach.Shared.Network
{
    public interface IServerConnection
    {
        event EventHandler<ErrorEventArgs> Error;
        event EventHandler Connected;
        event EventHandler Disconnected;
        event EventHandler<ChunkEventArgs> ChunkReceived;

        void Run();
        void ProcessMessages();
        void RequestChunks();
    }
}
