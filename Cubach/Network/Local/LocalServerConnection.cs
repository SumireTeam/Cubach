using System;
using System.Collections.Generic;
using Cubach.Model;

namespace Cubach.Network.Local
{
    public class LocalServerConnection : IServerConnection
    {
        private readonly Queue<ServerMessage> incomingMessages;
        private readonly Queue<ClientMessage> outgoingMessages;

        public event EventHandler<ErrorEventArgs> Error = (s, e) => { };
        public event EventHandler Connected = (s, e) => { };
        public event EventHandler Disconnected = (s, e) => { };
        public event EventHandler<ChunkEventArgs> ChunkReceived = (s, e) => { };

        public LocalServerConnection(Queue<ServerMessage> incomingMessages, Queue<ClientMessage> outgoingMessages)
        {
            this.incomingMessages = incomingMessages;
            this.outgoingMessages = outgoingMessages;
        }

        public void Run()
        {
            Connected(this, EventArgs.Empty);
        }

        public void ProcessMessages()
        {
            while (incomingMessages.Count > 0) {
                var msg = incomingMessages.Dequeue();
                if (msg is ChunkMessage chunkMessage) {
                    var chunk = chunkMessage.Chunk;
                    ChunkReceived(this, new ChunkEventArgs(chunk));
                }
            }
        }

        public void RequestChunks()
        {
            outgoingMessages.Enqueue(new ChunksRequest());
        }
    }
}
