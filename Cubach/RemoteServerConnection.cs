using System;
using Cubach.Model;

namespace Cubach
{
    public class RemoteServerConnection : IServerConnection
    {
        public event EventHandler<ChunkEventArgs> ChunkReceived = (s, e) => { };

        public void ProcessMessages()
        {
            throw new NotImplementedException();
        }

        public void RequestChunks()
        {
            throw new NotImplementedException();
        }
    }
}
